-- Table: public.audit_log

-- DROP TABLE IF EXISTS public.audit_log;

CREATE TABLE IF NOT EXISTS public.audit_log
(
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    table_name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    operation character varying(10) COLLATE pg_catalog."default" NOT NULL,
    row_id integer NOT NULL,
    user_id integer,
    old_values jsonb,
    new_values jsonb,
    changed_at timestamp without time zone NOT NULL DEFAULT now(),
    CONSTRAINT audit_log_pkey PRIMARY KEY (id),
    CONSTRAINT audit_log_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public.users (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE SET NULL
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.audit_log
    OWNER to postgres;
-- Index: IX_audit_log_user_id

-- DROP INDEX IF EXISTS public."IX_audit_log_user_id";

CREATE INDEX IF NOT EXISTS "IX_audit_log_user_id"
    ON public.audit_log USING btree
    (user_id ASC NULLS LAST)
    WITH (fillfactor=100, deduplicate_items=True)
    TABLESPACE pg_default;