-- =====================================================
-- CoffeeTea E-commerce Test Data Seed Script
-- =====================================================
-- This script populates the database with realistic test data
-- for comprehensive testing of the e-commerce platform
-- =====================================================

-- Clean up existing test data (preserve system data)
-- Note: Run this section carefully in production environments

-- DELETE FROM audit_log WHERE id > 0;
-- DELETE FROM shipments WHERE id > 0;
-- DELETE FROM payments WHERE id > 0;
-- DELETE FROM order_items WHERE id > 0;
-- DELETE FROM order_coupons WHERE order_id > 0;
-- DELETE FROM orders WHERE id > 0;
-- DELETE FROM cart WHERE id > 0;
-- DELETE FROM reviews WHERE id > 0;
-- DELETE FROM chat_messages WHERE id > 0;
-- DELETE FROM chat_threads WHERE id > 0;
-- DELETE FROM article_products WHERE article_id > 0;
-- DELETE FROM articles WHERE id > 0;
-- DELETE FROM article_categories WHERE id > 0;
-- DELETE FROM product_details WHERE id > 0;
-- DELETE FROM products WHERE id > 0;
-- DELETE FROM categories WHERE id > 0;
-- DELETE FROM coupons WHERE id > 0;
-- DELETE FROM addresses WHERE id > 0;
-- DELETE FROM users WHERE email NOT IN ('admin@coffee', 'consultant@coffee');

-- =====================================================
-- 1. Product Categories
-- =====================================================
INSERT INTO categories (name, parent_id, deleted) VALUES
('Кофе', NULL, false),
('Чай', NULL, false),
('Аксессуары', NULL, false);

-- Subcategories for Coffee
INSERT INTO categories (name, parent_id, deleted) VALUES
('Арабика', (SELECT id FROM categories WHERE name = 'Кофе' LIMIT 1), false),
('Робуста', (SELECT id FROM categories WHERE name = 'Кофе' LIMIT 1), false),
('Смеси', (SELECT id FROM categories WHERE name = 'Кофе' LIMIT 1), false);

-- Subcategories for Tea
INSERT INTO categories (name, parent_id, deleted) VALUES
('Зелёный чай', (SELECT id FROM categories WHERE name = 'Чай' LIMIT 1), false),
('Чёрный чай', (SELECT id FROM categories WHERE name = 'Чай' LIMIT 1), false),
('Травяной чай', (SELECT id FROM categories WHERE name = 'Чай' LIMIT 1), false),
('Улун', (SELECT id FROM categories WHERE name = 'Чай' LIMIT 1), false);

-- =====================================================
-- 2. Products (Coffee & Tea)
-- =====================================================

-- Coffee Products
INSERT INTO products (category_id, name, type, sku, price, quantity, description, image_url, roast_level, processing, origin_country, origin_region, deleted) VALUES
(
    (SELECT id FROM categories WHERE name = 'Арабика' LIMIT 1),
    'Эфиопия Йиргачеф',
    'coffee',
    'COFFEE-ETH-001',
    890.00,
    150,
    'Эфиопская арабика с яркими цветочными и цитрусовыми нотами. Считается родиной кофе.',
    '/images/products/ethiopia-yirgacheffe.jpg',
    'Светлая',
    'Мытая',
    'Эфиопия',
    'Йиргачеф',
    false
),
(
    (SELECT id FROM categories WHERE name = 'Арабика' LIMIT 1),
    'Колумбия Супремо',
    'coffee',
    'COFFEE-COL-002',
    750.00,
    200,
    'Сбалансированный колумбийский кофе с нотами карамели и орехов.',
    '/images/products/colombia-supremo.jpg',
    'Средняя',
    'Мытая',
    'Колумбия',
    'Антиокия',
    false
),
(
    (SELECT id FROM categories WHERE name = 'Робуста' LIMIT 1),
    'Вьетнам Робуста',
    'coffee',
    'COFFEE-VIE-003',
    450.00,
    300,
    'Крепкий вьетнамский робуста с шоколадными нотами и высоким содержанием кофеина.',
    '/images/products/vietnam-robusta.jpg',
    'Тёмная',
    'Сухая',
    'Вьетнам',
    'Далат',
    false
),
(
    (SELECT id FROM categories WHERE name = 'Смеси' LIMIT 1),
    'Эспрессо Блэнд',
    'coffee',
    'COFFEE-BLD-004',
    680.00,
    180,
    'Идеальная смесь для эспрессо: 70% арабика, 30% робуста. Плотное тело и крема.',
    '/images/products/espresso-blend.jpg',
    'Средне-тёмная',
    'Смешанная',
    'Смесь',
    'Мульти-регион',
    false
),
(
    (SELECT id FROM categories WHERE name = 'Арабика' LIMIT 1),
    'Бразилия Сантос',
    'coffee',
    'COFFEE-BRA-005',
    620.00,
    220,
    'Классический бразильский кофе с ореховым вкусом и низкой кислотностью.',
    '/images/products/brazil-santos.jpg',
    'Средняя',
    'Натуральная',
    'Бразилия',
    'Минас-Жерайс',
    false
);

-- Tea Products
INSERT INTO products (category_id, name, type, sku, price, quantity, description, image_url, roast_level, processing, origin_country, origin_region, deleted) VALUES
(
    (SELECT id FROM categories WHERE name = 'Зелёный чай' LIMIT 1),
    'Сенча Премиум',
    'tea',
    'TEA-JAP-001',
    1200.00,
    100,
    'Японский зелёный чай с свежим травянистым вкусом и нежной сладостью.',
    '/images/products/sencha-premium.jpg',
    NULL,
    'Пропаривание',
    'Япония',
    'Сидзуока',
    false
),
(
    (SELECT id FROM categories WHERE name = 'Чёрный чай' LIMIT 1),
    'Ассам GFOP',
    'tea',
    'TEA-IND-002',
    850.00,
    150,
    'Индийский чёрный чай с солодовым вкусом и крепким настоем.',
    '/images/products/assam-gfop.jpg',
    NULL,
    'Ортодоксальная',
    'Индия',
    'Ассам',
    false
),
(
    (SELECT id FROM categories WHERE name = 'Улун' LIMIT 1),
    'Те Гуань Инь',
    'tea',
    'TEA-CHN-003',
    1500.00,
    80,
    'Классический китайский улун с цветочным ароматом и медовой сладостью.',
    '/images/products/tie-guan-yin.jpg',
    NULL,
    'Полуферментированная',
    'Китай',
    'Фуцзянь',
    false
),
(
    (SELECT id FROM categories WHERE name = 'Зелёный чай' LIMIT 1),
    'Лунцзин (Колодец Дракона)',
    'tea',
    'TEA-CHN-004',
    1800.00,
    60,
    'Элитный китайский зелёный чай с ореховым вкусом и длительным послевкусием.',
    '/images/products/longjing.jpg',
    NULL,
    'Обжарка в воке',
    'Китай',
    'Чжэцзян',
    false
),
(
    (SELECT id FROM categories WHERE name = 'Травяной чай' LIMIT 1),
    'Ройбуш Ванильный',
    'tea',
    'TEA-SAF-005',
    550.00,
    120,
    'Южноафриканский травяной чай без кофеина с нотами ванили.',
    '/images/products/rooibos-vanilla.jpg',
    NULL,
    'Ферментация',
    'ЮАР',
    'Кедарберг',
    false
);

-- =====================================================
-- 3. Product Details (Extended Information)
-- =====================================================
INSERT INTO product_details (product_id, deleted) VALUES
((SELECT id FROM products WHERE sku = 'COFFEE-ETH-001' LIMIT 1), false),
((SELECT id FROM products WHERE sku = 'COFFEE-COL-002' LIMIT 1), false),
((SELECT id FROM products WHERE sku = 'TEA-JAP-001' LIMIT 1), false),
((SELECT id FROM products WHERE sku = 'TEA-CHN-003' LIMIT 1), false);

-- =====================================================
-- 4. Test Users (Customers)
-- =====================================================
-- Password for all test users: Test123!
INSERT INTO users (role_id, last_name, first_name, middle_name, email, phone, password_hash, created_at, deleted) VALUES
(
    (SELECT id FROM roles WHERE name = 'user' LIMIT 1),
    'Иванов',
    'Иван',
    'Иванович',
    'ivan.ivanov@test.ru',
    '+7 (900) 123-45-67',
    '$2a$11$vXeVF4QdKj5wC.lzF7u0KuJxKjPqLk5yH7.xGv0aN8zPzwKN6JYc6',
    NOW(),
    false
),
(
    (SELECT id FROM roles WHERE name = 'user' LIMIT 1),
    'Петрова',
    'Мария',
    'Сергеевна',
    'maria.petrova@test.ru',
    '+7 (901) 234-56-78',
    '$2a$11$vXeVF4QdKj5wC.lzF7u0KuJxKjPqLk5yH7.xGv0aN8zPzwKN6JYc6',
    NOW(),
    false
),
(
    (SELECT id FROM roles WHERE name = 'user' LIMIT 1),
    'Сидоров',
    'Алексей',
    'Дмитриевич',
    'alexey.sidorov@test.ru',
    '+7 (902) 345-67-89',
    '$2a$11$vXeVF4QdKj5wC.lzF7u0KuJxKjPqLk5yH7.xGv0aN8zPzwKN6JYc6',
    NOW(),
    false
);

-- =====================================================
-- 5. User Addresses
-- =====================================================
INSERT INTO addresses (user_id, line1, line2, city, postal_code, country, is_default, deleted) VALUES
(
    (SELECT id FROM users WHERE email = 'ivan.ivanov@test.ru' LIMIT 1),
    'ул. Ленина, д. 25, кв. 10',
    NULL,
    'Москва',
    '101000',
    'Россия',
    true,
    false
),
(
    (SELECT id FROM users WHERE email = 'maria.petrova@test.ru' LIMIT 1),
    'Невский проспект, д. 100, кв. 5',
    NULL,
    'Санкт-Петербург',
    '190000',
    'Россия',
    true,
    false
),
(
    (SELECT id FROM users WHERE email = 'alexey.sidorov@test.ru' LIMIT 1),
    'пр. Мира, д. 50, кв. 20',
    'подъезд 2',
    'Екатеринбург',
    '620000',
    'Россия',
    true,
    false
);

-- =====================================================
-- 6. Shopping Cart Items
-- =====================================================
INSERT INTO cart (user_id, product_id, quantity, price, added_at, deleted) VALUES
(
    (SELECT id FROM users WHERE email = 'ivan.ivanov@test.ru' LIMIT 1),
    (SELECT id FROM products WHERE sku = 'COFFEE-ETH-001' LIMIT 1),
    2,
    890.00,
    NOW(),
    false
),
(
    (SELECT id FROM users WHERE email = 'ivan.ivanov@test.ru' LIMIT 1),
    (SELECT id FROM products WHERE sku = 'TEA-JAP-001' LIMIT 1),
    1,
    1200.00,
    NOW(),
    false
),
(
    (SELECT id FROM users WHERE email = 'maria.petrova@test.ru' LIMIT 1),
    (SELECT id FROM products WHERE sku = 'COFFEE-COL-002' LIMIT 1),
    3,
    750.00,
    NOW(),
    false
);

-- =====================================================
-- 7. Coupons / Discount Codes
-- =====================================================
INSERT INTO coupons (code, discount_type, value, min_total, valid_from, valid_to, deleted) VALUES
('WELCOME10', 'percentage', 10.00, 500.00, NOW() - INTERVAL '7 days', NOW() + INTERVAL '30 days', false),
('FREESHIP', 'fixed', 300.00, 1500.00, NOW() - INTERVAL '3 days', NOW() + INTERVAL '14 days', false),
('NEWYEAR2025', 'percentage', 25.00, 2000.00, NOW() - INTERVAL '1 day', NOW() + INTERVAL '60 days', false),
('EXPIRED50', 'percentage', 50.00, 1000.00, NOW() - INTERVAL '60 days', NOW() - INTERVAL '30 days', false);

-- =====================================================
-- 8. Orders
-- =====================================================
INSERT INTO orders (user_id, address_id, status_id, total, payment_status, created_at, deleted) VALUES
(
    (SELECT id FROM users WHERE email = 'ivan.ivanov@test.ru' LIMIT 1),
    (SELECT id FROM addresses WHERE user_id = (SELECT id FROM users WHERE email = 'ivan.ivanov@test.ru' LIMIT 1) LIMIT 1),
    (SELECT id FROM order_statuses WHERE name = 'Завершён' LIMIT 1),
    2530.00,
    'paid',
    NOW() - INTERVAL '10 days',
    false
),
(
    (SELECT id FROM users WHERE email = 'maria.petrova@test.ru' LIMIT 1),
    (SELECT id FROM addresses WHERE user_id = (SELECT id FROM users WHERE email = 'maria.petrova@test.ru' LIMIT 1) LIMIT 1),
    (SELECT id FROM order_statuses WHERE name = 'Отправлен' LIMIT 1),
    3120.00,
    'paid',
    NOW() - INTERVAL '5 days',
    false
),
(
    (SELECT id FROM users WHERE email = 'alexey.sidorov@test.ru' LIMIT 1),
    (SELECT id FROM addresses WHERE user_id = (SELECT id FROM users WHERE email = 'alexey.sidorov@test.ru' LIMIT 1) LIMIT 1),
    (SELECT id FROM order_statuses WHERE name = 'В обработке' LIMIT 1),
    1450.00,
    'pending',
    NOW() - INTERVAL '1 day',
    false
);

-- =====================================================
-- 9. Order Items
-- =====================================================
-- Order 1 items
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
(1, (SELECT id FROM products WHERE sku = 'COFFEE-ETH-001' LIMIT 1), 2, 890.00),
(1, (SELECT id FROM products WHERE sku = 'TEA-CHN-003' LIMIT 1), 1, 1500.00);

-- Order 2 items
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
(2, (SELECT id FROM products WHERE sku = 'COFFEE-COL-002' LIMIT 1), 3, 750.00),
(2, (SELECT id FROM products WHERE sku = 'TEA-JAP-001' LIMIT 1), 1, 1200.00);

-- Order 3 items
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
(3, (SELECT id FROM products WHERE sku = 'COFFEE-BLD-004' LIMIT 1), 2, 680.00);

-- =====================================================
-- 10. Payments
-- =====================================================
INSERT INTO payments (order_id, amount, method, status, provider_txn_id, paid_at) VALUES
(1, 2530.00, 'card', 'completed', 'TXN-001-2024-12', NOW() - INTERVAL '10 days'),
(2, 3120.00, 'card', 'completed', 'TXN-002-2024-12', NOW() - INTERVAL '5 days');

-- =====================================================
-- 11. Shipments
-- =====================================================
INSERT INTO shipments (order_id, carrier, tracking_number, status, shipped_at, delivered_at) VALUES
(
    1,
    'СДЭК',
    'CDEK-123456789',
    'delivered',
    NOW() - INTERVAL '9 days',
    NOW() - INTERVAL '5 days'
),
(
    2,
    'Почта России',
    'RF-987654321',
    'in_transit',
    NOW() - INTERVAL '4 days',
    NULL
);

-- =====================================================
-- 12. Product Reviews
-- =====================================================
INSERT INTO reviews (product_id, user_id, rating, text, created_at, is_moderated, deleted) VALUES
(
    (SELECT id FROM products WHERE sku = 'COFFEE-ETH-001' LIMIT 1),
    (SELECT id FROM users WHERE email = 'ivan.ivanov@test.ru' LIMIT 1),
    5,
    'Превосходный кофе! Яркий цветочный аромат и приятная кислинка. Рекомендую для альтернативных методов заваривания.',
    NOW() - INTERVAL '5 days',
    true,
    false
),
(
    (SELECT id FROM products WHERE sku = 'TEA-JAP-001' LIMIT 1),
    (SELECT id FROM users WHERE email = 'maria.petrova@test.ru' LIMIT 1),
    4,
    'Хорошая сенча, но немного дороговато. Вкус свежий, травянистый. Заваривается отлично.',
    NOW() - INTERVAL '3 days',
    true,
    false
),
(
    (SELECT id FROM products WHERE sku = 'COFFEE-COL-002' LIMIT 1),
    (SELECT id FROM users WHERE email = 'alexey.sidorov@test.ru' LIMIT 1),
    5,
    'Классическая колумбия! Сбалансированный вкус, идеально для утреннего эспрессо.',
    NOW() - INTERVAL '2 days',
    true,
    false
);

-- =====================================================
-- 13. Article Categories
-- =====================================================
INSERT INTO article_categories (name, deleted) VALUES
('Гайды по завариванию', false),
('История кофе и чая', false),
('Обзоры продуктов', false),
('Новости', false);

-- =====================================================
-- 14. Articles
-- =====================================================
INSERT INTO articles (category_id, title, slug, cover_image_url, summary, content, published_at, is_published, deleted) VALUES
(
    (SELECT id FROM article_categories WHERE name = 'Гайды по завариванию' LIMIT 1),
    'Как правильно заваривать эфиопский кофе',
    'kak-pravilno-zavarivat-efiopskij-kofe',
    '/images/articles/brewing-ethiopian.jpg',
    'Подробный гайд по завариванию эфиопского кофе в разных методах',
    '<h2>Введение</h2><p>Эфиопский кофе требует особого подхода к завариванию...</p><h2>Методы заваривания</h2><p>V60, Chemex, Аэропресс...</p>',
    NOW() - INTERVAL '15 days',
    true,
    false
),
(
    (SELECT id FROM article_categories WHERE name = 'История кофе и чая' LIMIT 1),
    'История японского чая',
    'istoriya-yaponskogo-chaya',
    '/images/articles/japanese-tea-history.jpg',
    'Путешествие в историю японской чайной культуры',
    '<h2>Происхождение</h2><p>Чай появился в Японии в 9 веке...</p>',
    NOW() - INTERVAL '20 days',
    true,
    false
),
(
    (SELECT id FROM article_categories WHERE name = 'Обзоры продуктов' LIMIT 1),
    'Сравнение арабики и робусты',
    'sravnenie-arabiki-i-robusty',
    '/images/articles/arabica-vs-robusta.jpg',
    'Полное сравнение двух основных видов кофе',
    '<h2>Отличия</h2><p>Арабика и робуста различаются по многим параметрам...</p>',
    NOW() - INTERVAL '10 days',
    true,
    false
);

-- =====================================================
-- 15. Article-Product Associations
-- =====================================================
INSERT INTO article_products (article_id, product_id) VALUES
(
    (SELECT id FROM articles WHERE slug = 'kak-pravilno-zavarivat-efiopskij-kofe' LIMIT 1),
    (SELECT id FROM products WHERE sku = 'COFFEE-ETH-001' LIMIT 1)
),
(
    (SELECT id FROM articles WHERE slug = 'istoriya-yaponskogo-chaya' LIMIT 1),
    (SELECT id FROM products WHERE sku = 'TEA-JAP-001' LIMIT 1)
);

-- =====================================================
-- 16. Chat Threads
-- =====================================================
INSERT INTO chat_threads (client_id, consultant_id, status, created_at) VALUES
(
    (SELECT id FROM users WHERE email = 'ivan.ivanov@test.ru' LIMIT 1),
    (SELECT id FROM users WHERE email = 'consultant@coffee' LIMIT 1),
    'closed',
    NOW() - INTERVAL '7 days'
),
(
    (SELECT id FROM users WHERE email = 'maria.petrova@test.ru' LIMIT 1),
    (SELECT id FROM users WHERE email = 'consultant@coffee' LIMIT 1),
    'open',
    NOW() - INTERVAL '2 hours'
);

-- =====================================================
-- 17. Chat Messages
-- =====================================================
INSERT INTO chat_messages (thread_id, sender_id, message, created_at) VALUES
-- Thread 1
(1, (SELECT id FROM users WHERE email = 'ivan.ivanov@test.ru' LIMIT 1),
 'Здравствуйте! Какой кофе вы порекомендуете для турки?',
 NOW() - INTERVAL '7 days'),
(1, (SELECT id FROM users WHERE email = 'consultant@coffee' LIMIT 1),
 'Здравствуйте! Для турки отлично подойдёт Эфиопия Йиргачеф или Бразилия Сантос.',
 NOW() - INTERVAL '7 days' + INTERVAL '5 minutes'),
(1, (SELECT id FROM users WHERE email = 'ivan.ivanov@test.ru' LIMIT 1),
 'Спасибо! Закажу Эфиопию.',
 NOW() - INTERVAL '7 days' + INTERVAL '10 minutes'),
-- Thread 2
(2, (SELECT id FROM users WHERE email = 'maria.petrova@test.ru' LIMIT 1),
 'Добрый день! Есть ли у вас чай без кофеина?',
 NOW() - INTERVAL '2 hours'),
(2, (SELECT id FROM users WHERE email = 'consultant@coffee' LIMIT 1),
 'Здравствуйте! Да, у нас есть травяной чай Ройбуш – он не содержит кофеина.',
 NOW() - INTERVAL '1 hour 55 minutes');

-- =====================================================
-- Verification Queries
-- =====================================================

-- Count records in each table
SELECT 'categories' as table_name, COUNT(*) as count FROM categories WHERE NOT deleted
UNION ALL
SELECT 'products', COUNT(*) FROM products WHERE NOT deleted
UNION ALL
SELECT 'users', COUNT(*) FROM users WHERE NOT deleted
UNION ALL
SELECT 'addresses', COUNT(*) FROM addresses WHERE NOT deleted
UNION ALL
SELECT 'cart', COUNT(*) FROM cart WHERE NOT deleted
UNION ALL
SELECT 'orders', COUNT(*) FROM orders WHERE NOT deleted
UNION ALL
SELECT 'order_items', COUNT(*) FROM order_items
UNION ALL
SELECT 'payments', COUNT(*) FROM payments
UNION ALL
SELECT 'shipments', COUNT(*) FROM shipments
UNION ALL
SELECT 'reviews', COUNT(*) FROM reviews WHERE NOT deleted
UNION ALL
SELECT 'articles', COUNT(*) FROM articles WHERE NOT deleted
UNION ALL
SELECT 'chat_threads', COUNT(*) FROM chat_threads
UNION ALL
SELECT 'chat_messages', COUNT(*) FROM chat_messages
UNION ALL
SELECT 'coupons', COUNT(*) FROM coupons WHERE NOT deleted
ORDER BY table_name;
