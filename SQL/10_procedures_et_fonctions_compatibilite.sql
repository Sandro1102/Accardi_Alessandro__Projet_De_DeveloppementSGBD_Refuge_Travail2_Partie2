create or replace function public.compatibilite_obtenir()
    returns TABLE(id integer, nom character varying)
    language plpgsql
as
$$
BEGIN
    RETURN QUERY
    SELECT identifiant, type
    FROM compatibilite;
END;
$$;

alter function public.compatibilite_obtenir() owner to postgres;

