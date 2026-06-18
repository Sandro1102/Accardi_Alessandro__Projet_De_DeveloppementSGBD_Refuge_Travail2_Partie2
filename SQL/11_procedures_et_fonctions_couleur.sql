create or replace function public.couleur_obtenir()
    returns TABLE(id integer, nom character varying)
    language plpgsql
as
$$
BEGIN
    RETURN QUERY
    SELECT col_identifiant, nom_couleur
    FROM couleur;
END;
$$;

alter function public.couleur_obtenir() owner to postgres;

