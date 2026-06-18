create or replace procedure public.ani_compatibilite_insertion(IN p_valeur boolean, IN p_description text, IN p_identifiant_compatibilite integer, IN p_identifiant_animal character varying)
    language plpgsql
as
$$
DECLARE
    existe_comp   compatibilite%ROWTYPE;
    existe_animal animal%ROWTYPE;
BEGIN
    -- Vérification compatibilité
    SELECT *
    INTO existe_comp
    FROM compatibilite
    WHERE identifiant = p_identifiant_compatibilite;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion impossible : compatibilité %s introuvable.',
                p_identifiant_compatibilite
                                        );
    END IF;

    -- Vérification animal
    SELECT *
    INTO existe_animal
    FROM animal
    WHERE identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion impossible : animal %s introuvable.',
                p_identifiant_animal
                                        );
    END IF;

    -- Insertion
    INSERT INTO ani_compatibilite (valeur,
                                   description,
                                   comp_identifiant,
                                   ani_identifiant)
    VALUES (p_valeur,
            p_description,
            p_identifiant_compatibilite,
            p_identifiant_animal);
END;
$$;

alter procedure public.ani_compatibilite_insertion(boolean, text, integer, varchar) owner to postgres;

create or replace procedure public.ani_compatibilite_modification(IN p_valeur boolean, IN p_description text, IN p_identifiant_compatibilite integer, IN p_identifiant_animal character varying)
    language plpgsql
as
$$
DECLARE
    existe ani_compatibilite%ROWTYPE;
BEGIN
    -- Vérification existence
    SELECT *
    INTO existe
    FROM ani_compatibilite
    WHERE comp_identifiant = p_identifiant_compatibilite
      AND ani_identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Impossible de modifier : aucune compatibilité trouvée pour comp_id=%s et ani_id=%s',
                p_identifiant_compatibilite,
                p_identifiant_animal
                                        );
    END IF;

    -- Modification
    UPDATE ani_compatibilite
    SET valeur      = p_valeur,
        description = p_description
    WHERE comp_identifiant = p_identifiant_compatibilite
      AND ani_identifiant = p_identifiant_animal;
END;
$$;

alter procedure public.ani_compatibilite_modification(boolean, text, integer, varchar) owner to postgres;

create or replace procedure public.ani_compatibilite_suppression(IN p_identifiant_compatibilite integer, IN p_identifiant_animal character varying)
    language plpgsql
as
$$
DECLARE
    existe ani_compatibilite%ROWTYPE;
BEGIN
    -- Vérification existence
    SELECT *
    INTO existe
    FROM ani_compatibilite
    WHERE comp_identifiant = p_identifiant_compatibilite
      AND ani_identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Impossible de supprimer : aucune compatibilité trouvée pour comp_id=%s et ani_id=%s',
                p_identifiant_compatibilite,
                p_identifiant_animal
                                        );
    END IF;

    -- Suppression
    DELETE
    FROM ani_compatibilite
    WHERE comp_identifiant = p_identifiant_compatibilite
      AND ani_identifiant = p_identifiant_animal;
END;
$$;

alter procedure public.ani_compatibilite_suppression(integer, varchar) owner to postgres;

