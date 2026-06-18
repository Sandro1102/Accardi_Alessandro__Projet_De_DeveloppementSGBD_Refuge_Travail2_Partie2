create or replace function public.role_obtenir()
    returns TABLE(id integer, nom character varying)
    language plpgsql
as
$$
BEGIN
    RETURN QUERY
    SELECT rol_identifiant, rol_nom
    FROM role;
END;
$$;

alter function public.role_obtenir() owner to postgres;

