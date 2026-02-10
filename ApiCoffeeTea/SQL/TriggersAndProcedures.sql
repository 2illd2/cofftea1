
-- =========================
-- ТРИГГЕРЫ ДЛЯ ПРОВЕРОК
-- =========================

-- Триггер: проверка total > 0 при вставке заказа
CREATE OR REPLACE FUNCTION check_order_total()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.total <= 0 THEN
        RAISE EXCEPTION 'Сумма заказа должна быть больше 0. Получено: %', NEW.total;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_check_order_total ON orders;
CREATE TRIGGER tr_check_order_total
    BEFORE INSERT ON orders
    FOR EACH ROW
    EXECUTE FUNCTION check_order_total();

-- Триггер: проверка, что пользователь покупал товар перед отзывом
CREATE OR REPLACE FUNCTION check_review_purchase()
RETURNS TRIGGER AS $$
BEGIN
    -- Проверяем, покупал ли пользователь этот товар
    IF NOT EXISTS (
        SELECT 1
        FROM orders o
        JOIN order_items oi ON o.id = oi.order_id
        WHERE o.user_id = NEW.user_id
        AND oi.product_id = NEW.product_id
        AND o.payment_status = 'paid'
        AND o.deleted = FALSE
    ) THEN
        RAISE EXCEPTION 'Нельзя оставить отзыв на товар, который вы не покупали';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_check_review_purchase ON reviews;
CREATE TRIGGER tr_check_review_purchase
    BEFORE INSERT ON reviews
    FOR EACH ROW
    EXECUTE FUNCTION check_review_purchase();

-- =========================
-- ТРИГГЕР ДЛЯ АУДИТА ЗАКАЗОВ
-- =========================
CREATE OR REPLACE FUNCTION audit_orders()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO audit_log (table_name, operation, row_id, user_id, new_values)
        VALUES ('orders', 'INSERT', NEW.id, NEW.user_id, row_to_json(NEW)::jsonb);
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO audit_log (table_name, operation, row_id, user_id, old_values, new_values)
        VALUES ('orders', 'UPDATE', NEW.id, NEW.user_id, row_to_json(OLD)::jsonb, row_to_json(NEW)::jsonb);
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO audit_log (table_name, operation, row_id, user_id, old_values)
        VALUES ('orders', 'DELETE', OLD.id, OLD.user_id, row_to_json(OLD)::jsonb);
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_audit_orders ON orders;
CREATE TRIGGER tr_audit_orders
    AFTER INSERT OR UPDATE OR DELETE ON orders
    FOR EACH ROW
    EXECUTE FUNCTION audit_orders();

-- =========================
-- ХРАНИМЫЕ ПРОЦЕДУРЫ
-- =========================

-- 1. Уменьшение остатков после заказа
CREATE OR REPLACE FUNCTION sp_UpdateStockAfterOrder(p_order_id INT)
RETURNS VOID AS $$
DECLARE
    item_record RECORD;
BEGIN
    -- Проходим по всем позициям заказа
    FOR item_record IN
        SELECT product_id, qty
        FROM order_items
        WHERE order_id = p_order_id
    LOOP
        -- Уменьшаем остаток
        UPDATE products
        SET quantity = quantity - item_record.qty
        WHERE id = item_record.product_id;

        -- Проверяем, что остаток не стал отрицательным
        IF (SELECT quantity FROM products WHERE id = item_record.product_id) < 0 THEN
            RAISE EXCEPTION 'Недостаточно товара на складе для продукта ID: %', item_record.product_id;
        END IF;
    END LOOP;

    RAISE NOTICE 'Остатки обновлены для заказа %', p_order_id;
END;
$$ LANGUAGE plpgsql;

-- 2. Топ популярных товаров
CREATE OR REPLACE FUNCTION sp_GetTopProducts(p_limit INT DEFAULT 10)
RETURNS TABLE(
    product_id INT,
    product_name VARCHAR(200),
    total_sold BIGINT,
    revenue DECIMAL(10,2)
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.id,
        p.name,
        SUM(oi.qty) as total_sold,
        SUM(oi.qty * oi.unit_price) as revenue
    FROM products p
    JOIN order_items oi ON p.id = oi.product_id
    JOIN orders o ON oi.order_id = o.id
    WHERE o.payment_status = 'paid'
    AND p.deleted = FALSE
    AND o.deleted = FALSE
    GROUP BY p.id, p.name
    ORDER BY total_sold DESC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- 3. Применение купона к заказу
CREATE OR REPLACE FUNCTION sp_ApplyCoupon(p_order_id INT, p_coupon_code VARCHAR(50))
RETURNS DECIMAL(10,2) AS $$
DECLARE
    v_coupon_id INT;
    v_discount_type VARCHAR(20);
    v_discount_value DECIMAL(10,2);
    v_min_total DECIMAL(10,2);
    v_current_total DECIMAL(10,2);
    v_new_total DECIMAL(10,2);
    v_discount_amount DECIMAL(10,2);
BEGIN
    -- Получаем информацию о купоне
    SELECT id, discount_type, value, min_total
    INTO v_coupon_id, v_discount_type, v_discount_value, v_min_total
    FROM coupons
    WHERE code = p_coupon_code
    AND deleted = FALSE
    AND (valid_from IS NULL OR valid_from <= NOW())
    AND (valid_to IS NULL OR valid_to >= NOW());

    IF v_coupon_id IS NULL THEN
        RAISE EXCEPTION 'Купон не найден или недействителен: %', p_coupon_code;
    END IF;

    -- Получаем текущую сумму заказа
    SELECT total INTO v_current_total
    FROM orders
    WHERE id = p_order_id;

    -- Проверяем минимальную сумму
    IF v_min_total IS NOT NULL AND v_current_total < v_min_total THEN
        RAISE EXCEPTION 'Минимальная сумма для купона: %. Текущая сумма: %', v_min_total, v_current_total;
    END IF;

    -- Рассчитываем скидку
    IF v_discount_type = 'percent' THEN
        v_discount_amount := v_current_total * (v_discount_value / 100);
    ELSE -- fixed
        v_discount_amount := v_discount_value;
    END IF;

    v_new_total := v_current_total - v_discount_amount;

    -- Не даем сумме стать отрицательной
    IF v_new_total < 0 THEN
        v_new_total := 0;
    END IF;

    -- Обновляем заказ
    UPDATE orders
    SET total = v_new_total
    WHERE id = p_order_id;

    -- Записываем связь заказ-купон
    INSERT INTO order_coupons (order_id, coupon_id)
    VALUES (p_order_id, v_coupon_id)
    ON CONFLICT DO NOTHING;

    RETURN v_new_total;
END;
$$ LANGUAGE plpgsql;

-- Триггер: автоматическое обновление цены в корзине при изменении цены товара
CREATE OR REPLACE FUNCTION update_cart_price()
RETURNS TRIGGER AS $$
BEGIN
    -- Обновляем цену во всех корзинах для этого товара
    UPDATE cart
    SET price = NEW.price
    WHERE product_id = NEW.id
    AND deleted = FALSE;

    RAISE NOTICE 'Обновлены цены в корзинах для товара: %', NEW.name;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_update_cart_price ON products;
CREATE TRIGGER tr_update_cart_price
    AFTER UPDATE OF price ON products
    FOR EACH ROW
    WHEN (OLD.price IS DISTINCT FROM NEW.price)
    EXECUTE FUNCTION update_cart_price();

-- Триггер: контроль единственного адреса по умолчанию у пользователя
CREATE OR REPLACE FUNCTION check_default_address()
RETURNS TRIGGER AS $$
BEGIN
    -- Если устанавливаем адрес как основной
    IF NEW.is_default = TRUE THEN
        -- Снимаем флаг is_default с других адресов этого пользователя
        UPDATE addresses
        SET is_default = FALSE
        WHERE user_id = NEW.user_id
        AND id != NEW.id
        AND deleted = FALSE;
    END IF;

    -- Если это единственный активный адрес пользователя, делаем его основным
    IF NOT EXISTS (
        SELECT 1 FROM addresses
        WHERE user_id = NEW.user_id
        AND is_default = TRUE
        AND deleted = FALSE
        AND id != NEW.id
    ) THEN
        NEW.is_default := TRUE;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_check_default_address ON addresses;
CREATE TRIGGER tr_check_default_address
    BEFORE INSERT OR UPDATE ON addresses
    FOR EACH ROW
    EXECUTE FUNCTION check_default_address();

-- 4. Очистка старых корзин (старше N дней)
CREATE OR REPLACE FUNCTION sp_CleanOldCarts(p_days_old INT DEFAULT 30)
RETURNS INT AS $$
DECLARE
    v_deleted_count INT;
BEGIN
    -- Помечаем как удаленные корзины старше указанного количества дней
    UPDATE cart
    SET deleted = TRUE
    WHERE added_at < (NOW() - (p_days_old || ' days')::INTERVAL)
    AND deleted = FALSE;

    GET DIAGNOSTICS v_deleted_count = ROW_COUNT;

    RAISE NOTICE 'Очищено корзин: % (старше % дней)', v_deleted_count, p_days_old;
    RETURN v_deleted_count;
END;
$$ LANGUAGE plpgsql;

-- 5. Получение статистики пользователя (заказы, отзывы, активность)
CREATE OR REPLACE FUNCTION sp_GetUserStats(p_user_id INT)
RETURNS TABLE(
    user_name VARCHAR(300),
    total_orders BIGINT,
    total_spent DECIMAL(10,2),
    avg_order_value DECIMAL(10,2),
    reviews_count BIGINT,
    last_order_date TIMESTAMP,
    cart_items_count BIGINT,
    registration_date TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        CONCAT(u.first_name, ' ', u.last_name) as user_name,
        COUNT(DISTINCT o.id) as total_orders,
        COALESCE(SUM(o.total), 0) as total_spent,
        COALESCE(AVG(o.total), 0) as avg_order_value,
        COUNT(DISTINCT r.id) as reviews_count,
        MAX(o.created_at) as last_order_date,
        COUNT(DISTINCT c.id) as cart_items_count,
        u.created_at as registration_date
    FROM users u
    LEFT JOIN orders o ON u.id = o.user_id AND o.deleted = FALSE AND o.payment_status = 'paid'
    LEFT JOIN reviews r ON u.id = r.user_id AND r.deleted = FALSE
    LEFT JOIN cart c ON u.id = c.user_id AND c.deleted = FALSE
    WHERE u.id = p_user_id
    AND u.deleted = FALSE
    GROUP BY u.id, u.first_name, u.last_name, u.created_at;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION audit_products()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'UPDATE' THEN
        -- Логируем только значимые изменения (цена, количество, статус)
        IF (OLD.price IS DISTINCT FROM NEW.price) OR
           (OLD.quantity IS DISTINCT FROM NEW.quantity) OR
           (OLD.deleted IS DISTINCT FROM NEW.deleted) THEN

            INSERT INTO audit_log (table_name, operation, row_id, old_values, new_values)
            VALUES ('products', 'UPDATE', NEW.id,
                   json_build_object('price', OLD.price, 'quantity', OLD.quantity, 'deleted', OLD.deleted)::jsonb,
                   json_build_object('price', NEW.price, 'quantity', NEW.quantity, 'deleted', NEW.deleted)::jsonb);
        END IF;
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_audit_products ON products;
CREATE TRIGGER tr_audit_products
    AFTER UPDATE ON products
    FOR EACH ROW
    EXECUTE FUNCTION audit_products();

-- =========================
-- ПРОЦЕДУРЫ ДЛЯ РЕЗЕРВНОГО КОПИРОВАНИЯ
-- =========================

-- 6. Создание резервной копии таблиц (простой вариант)
CREATE OR REPLACE FUNCTION sp_CreateBackup()
RETURNS TEXT AS $$
DECLARE
    backup_name TEXT;
    result TEXT;
BEGIN
    backup_name := 'coffeetea_backup_' || to_char(NOW(), 'YYYY_MM_DD_HH24_MI_SS');

    EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', backup_name);

    EXECUTE format('CREATE TABLE %I.users_backup AS SELECT * FROM users', backup_name);
    EXECUTE format('CREATE TABLE %I.products_backup AS SELECT * FROM products', backup_name);
    EXECUTE format('CREATE TABLE %I.orders_backup AS SELECT * FROM orders', backup_name);
    EXECUTE format('CREATE TABLE %I.order_items_backup AS SELECT * FROM order_items', backup_name);
    EXECUTE format('CREATE TABLE %I.reviews_backup AS SELECT * FROM reviews', backup_name);
    EXECUTE format('CREATE TABLE %I.audit_log_backup AS SELECT * FROM audit_log', backup_name);

    result := format('Backup created: %s', backup_name);
    RAISE NOTICE '%', result;

    RETURN result;
END;
$$ LANGUAGE plpgsql;

-- 7. Получение списка бэкапов
CREATE OR REPLACE FUNCTION sp_ListBackups()
RETURNS TABLE(
    backup_name TEXT,
    created_at TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        schema_name::TEXT,
        NULL::TIMESTAMP as created_at
    FROM information_schema.schemata
    WHERE schema_name LIKE 'coffeetea_backup_%'
    ORDER BY schema_name DESC;
END;
$$ LANGUAGE plpgsql;

-- 8. Восстановление из резервной копии
CREATE OR REPLACE FUNCTION sp_RestoreFromBackup(backup_schema_name TEXT)
RETURNS TEXT AS $$
DECLARE
    result TEXT;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = backup_schema_name) THEN
        RAISE EXCEPTION 'Backup schema % does not exist', backup_schema_name;
    END IF;


    TRUNCATE users, products, orders, order_items, reviews, audit_log RESTART IDENTITY CASCADE;

    EXECUTE format('INSERT INTO users SELECT * FROM %I.users_backup', backup_schema_name);
    EXECUTE format('INSERT INTO products SELECT * FROM %I.products_backup', backup_schema_name);
    EXECUTE format('INSERT INTO orders SELECT * FROM %I.orders_backup', backup_schema_name);
    EXECUTE format('INSERT INTO order_items SELECT * FROM %I.order_items_backup', backup_schema_name);
    EXECUTE format('INSERT INTO reviews SELECT * FROM %I.reviews_backup', backup_schema_name);
    EXECUTE format('INSERT INTO audit_log SELECT * FROM %I.audit_log_backup', backup_schema_name);

    result := format('Data restored from backup: %s', backup_schema_name);
    RAISE NOTICE '%', result;

    RETURN result;
END;
$$ LANGUAGE plpgsql;

-- 9. Удаление бэкапа
CREATE OR REPLACE FUNCTION sp_DeleteBackup(backup_schema_name TEXT)
RETURNS TEXT AS $$
DECLARE
    result TEXT;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = backup_schema_name) THEN
        RAISE EXCEPTION 'Backup schema % does not exist', backup_schema_name;
    END IF;

    EXECUTE format('DROP SCHEMA %I CASCADE', backup_schema_name);

    result := format('Backup deleted: %s', backup_schema_name);
    RAISE NOTICE '%', result;

    RETURN result;
END;
$$ LANGUAGE plpgsql;


