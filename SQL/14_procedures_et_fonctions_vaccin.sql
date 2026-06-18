create or replace function public.vaccin_obtenir()
    returns TABLE(id integer, nom character varying)
    language plpgsql
as
$$
BEGIN
    RETURN QUERY
    SELECT identifiant, nom
    FROM vaccin;
END;
$$;

alter function public.vaccin_obtenir() owner to postgres;

