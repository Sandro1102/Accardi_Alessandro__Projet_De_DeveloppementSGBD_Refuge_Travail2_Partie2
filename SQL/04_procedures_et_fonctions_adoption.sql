create or replace procedure public.adoption_insertion(OUT p_adoption_id integer, IN p_statut character varying, IN p_date_demande date, IN p_identifiant_animal character varying, IN p_identifiant_contact integer)
    language plpgsql
as
$$
DECLARE
    existe_animal    animal%ROWTYPE;
    existe_contact   contact%ROWTYPE;
    adoption_active  adoption%ROWTYPE;
    fa_active        famille_accueil%ROWTYPE;
BEGIN
    -- Vérification animal
    SELECT *
    INTO existe_animal
    FROM animal
    WHERE identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion adoption impossible : animal %s introuvable.',
                p_identifiant_animal
                                        );
    END IF;

    -- Vérification contact
    SELECT *
    INTO existe_contact
    FROM contact
    WHERE contact_identifiant = p_identifiant_contact;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion adoption impossible : contact %s introuvable.',
                p_identifiant_contact
                                        );
    END IF;

    -- Contrainte : un animal décédé ne peut pas avoir de demande ou d''adoption acceptée
    IF existe_animal.date_deces IS NOT NULL THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion adoption impossible : animal %s est décédé le %s.',
                p_identifiant_animal,
                existe_animal.date_deces
                                        );
    END IF;

    -- Contrainte : statut valide
    IF p_statut NOT IN ('demande', 'acceptee', 'rejet_environnement', 'rejet_comportement') THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion adoption impossible : statut %s invalide.',
                p_statut
                                        );
    END IF;

    -- Contrainte : vérifier qu'il n'existe pas déjà une demande en cours ou une adoption acceptée
    -- pour cet animal. Une nouvelle demande n'est possible que si l'animal est revenu au refuge
    -- via une entrée avec raison ''retour_adoption'' après une adoption acceptée.
    SELECT *
    INTO adoption_active
    FROM adoption
    WHERE ani_identifiant = p_identifiant_animal
      AND statut IN ('demande', 'acceptee')
    ORDER BY date_demande DESC
    LIMIT 1;

    IF FOUND THEN
        IF adoption_active.statut = 'demande' THEN
            RAISE EXCEPTION USING MESSAGE = FORMAT(
                    'Insertion adoption impossible : une demande d''adoption est déjà en cours pour l''animal %s.',
                    p_identifiant_animal
                                            );
        END IF;

        -- il ne faut rentrer dans ce if que si et seulement si une adoption à le statut acceptée sans quoi je vais bloquer toutes les autres
        -- demandes avec le test réalisé juste après.
        IF adoption_active.statut = 'acceptee' THEN
            -- Vérifier si l'animal est revenu au refuge via retour_adoption
            -- ce qui signifie qu'une nouvelle demande est possible
            -- le if not exits se traduit par : est-ce qu'au moins une ligne correspond à ma recherche ?
            -- Comme le test est inversé avec le not exists si aucune ligne n'est trouvé suite à ma recherche l'animal n'est pas au refuge.
            -- il n'est pas revenu d'une adoption
            IF NOT EXISTS (
                SELECT 1
                FROM ani_entree
                WHERE ani_identifiant = p_identifiant_animal
                  AND raison = 'retour_adoption'
                  AND date_entree > adoption_active.date_demande
            ) THEN
                RAISE EXCEPTION USING MESSAGE = FORMAT(
                        'Insertion adoption impossible : animal %s a déjà été adopté et n''est pas revenu au refuge.',
                        p_identifiant_animal
                                                );
            END IF;
        END IF;
    END IF;

    -- Contrainte : si l'animal est en famille d'accueil sans date de retour fixée
    -- il faut attendre son retour avant d'introduire une demande d''adoption
    SELECT *
    INTO fa_active
    FROM famille_accueil
    WHERE fa_ani_identifiant = p_identifiant_animal
      AND date_fin IS NULL
    ORDER BY date_debut DESC
    LIMIT 1;

    IF FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion adoption impossible : animal %s est en famille d''accueil sans date de retour fixée.',
                p_identifiant_animal
                                        );
    END IF;

    -- Contrainte : si l'animal est revenu d'une famille d'accueil,
    -- la date de demande ne peut pas être inférieure à la date de retour
    SELECT *
    INTO fa_active
    FROM famille_accueil
    WHERE fa_ani_identifiant = p_identifiant_animal
      AND date_fin IS NOT NULL
    ORDER BY date_fin DESC
    LIMIT 1;

    IF FOUND AND p_date_demande < fa_active.date_fin THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion adoption impossible : la date de demande %s est antérieure à la date de retour de famille d''accueil %s.',
                p_date_demande,
                fa_active.date_fin
                                        );
    END IF;

    -- Insertion
    INSERT INTO adoption (statut,
                          date_demande,
                          ani_identifiant,
                          adop_contact)
    VALUES (p_statut,
            p_date_demande,
            p_identifiant_animal,
            p_identifiant_contact)
    RETURNING adoption_id INTO p_adoption_id;  -- renvoie l''id généré par le SERIAL

END;
$$;

alter procedure public.adoption_insertion(out integer, varchar, date, varchar, integer) owner to postgres;

create or replace procedure public.adoption_modification(IN p_adoption_id integer, IN p_statut character varying, IN p_date_demande date, IN p_identifiant_animal character varying, IN p_identifiant_contact integer)
    language plpgsql
as
$$
DECLARE
    existe          adoption%ROWTYPE;
    existe_animal   animal%ROWTYPE;
    existe_contact  contact%ROWTYPE;
BEGIN
    -- Vérification existence adoption
    SELECT *
    INTO existe
    FROM adoption
    WHERE adoption_id = p_adoption_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification impossible : adoption %s introuvable.',
                p_adoption_id
                                        );
    END IF;

    -- Vérification animal
    SELECT *
    INTO existe_animal
    FROM animal
    WHERE identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification adoption impossible : animal %s introuvable.',
                p_identifiant_animal
                                        );
    END IF;

    -- Vérification contact
    SELECT *
    INTO existe_contact
    FROM contact
    WHERE contact_identifiant = p_identifiant_contact;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification adoption impossible : contact %s introuvable.',
                p_identifiant_contact
                                        );
    END IF;

    -- Contrainte : un animal décédé ne peut pas avoir de modification d''adoption
    IF existe_animal.date_deces IS NOT NULL THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification adoption impossible : animal %s est décédé le %s.',
                p_identifiant_animal,
                existe_animal.date_deces
                                        );
    END IF;

    -- Contrainte : statut valide
    IF p_statut NOT IN ('demande', 'acceptee', 'rejet_environnement', 'rejet_comportement') THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification adoption impossible : statut %s invalide.',
                p_statut
                                        );
    END IF;

    -- Contrainte : si le statut passe à ''acceptee'' une sortie avec raison ''adoption''
    -- sera insérée dans ani_sortie par le code C#. On vérifie donc qu'il n'existe pas
    -- déjà une sortie adoption plus récente que la dernière entrée pour cet animal.
    IF p_statut = 'acceptee' AND existe.statut != 'acceptee' THEN
        IF EXISTS (
            SELECT 1
            FROM ani_sortie so
            WHERE so.ani_identifiant = p_identifiant_animal
              AND so.raison = 'adoption'
              AND so.date_sortie >= (
                  SELECT MAX(date_entree)
                  FROM ani_entree
                  WHERE ani_identifiant = p_identifiant_animal
              )
        ) THEN
            RAISE EXCEPTION USING MESSAGE = FORMAT(
                    'Modification adoption impossible : animal %s a déjà une sortie adoption enregistrée.',
                    p_identifiant_animal
                                            );
        END IF;
    END IF;

    -- Modification
    UPDATE adoption
    SET statut          = p_statut,
        date_demande    = p_date_demande,
        ani_identifiant = p_identifiant_animal,
        adop_contact    = p_identifiant_contact
    WHERE adoption_id = p_adoption_id;

END;
$$;

alter procedure public.adoption_modification(integer, varchar, date, varchar, integer) owner to postgres;

create or replace procedure public.adoption_suppression(IN p_adoption_id integer)
    language plpgsql
as
$$
DECLARE
    existe adoption%ROWTYPE;
BEGIN
    -- Vérification existence
    SELECT *
    INTO existe
    FROM adoption
    WHERE adoption_id = p_adoption_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Suppression impossible : adoption %s introuvable.',
                p_adoption_id
                                        );
    END IF;

    -- Contrainte : une adoption acceptée ne peut pas être supprimée
    -- car elle a généré une sortie dans ani_sortie. Il faut d''abord
    -- traiter le retour de l''animal via ani_entree avec raison retour_adoption.
    IF existe.statut = 'acceptee' THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Suppression impossible : adoption %s a le statut acceptee. Enregistrez d''abord le retour de l''animal.',
                p_adoption_id
                                        );
    END IF;

    -- Suppression
    DELETE
    FROM adoption
    WHERE adoption_id = p_adoption_id;

END;
$$;

alter procedure public.adoption_suppression(integer) owner to postgres;

create or replace function public.adoption_afficher_liste_complete()
    returns TABLE(adoption_id integer, statut character varying, date_demande date, ani_identifiant character varying, adop_contact integer)
    language plpgsql
as
$$
BEGIN

    RETURN QUERY
        SELECT
            a.adoption_id,
            a.statut,
            a.date_demande,
            a.ani_identifiant,
            a.adop_contact
        FROM adoption a;

END;
$$;

alter function public.adoption_afficher_liste_complete() owner to postgres;

create or replace function public.adoption_afficher_un(p_adoption_id integer)
    returns TABLE(adoption_id integer, statut character varying, date_demande date, ani_identifiant character varying, ani_nom character varying, adop_contact integer, contact_nom character varying, contact_prenom character varying)
    language plpgsql
as
$$
DECLARE
    existe adoption%ROWTYPE;
BEGIN
    -- Vérifier que l''adoption existe
    SELECT *
    INTO existe
    FROM adoption
    WHERE adoption_id = p_adoption_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION
            'L''adoption % n''est pas présente dans la liste.',
            p_adoption_id;
    END IF;

    -- Retourner l''adoption avec les informations de l''animal et du contact
    -- La jointure permet d''éviter plusieurs appels depuis le C#
    RETURN QUERY
    SELECT
        ad.adoption_id,
        ad.statut,
        ad.date_demande,
        ad.ani_identifiant,
        a.nom   AS ani_nom,
        ad.adop_contact,
        c.nom   AS contact_nom,
        c.prenom AS contact_prenom
    FROM adoption ad
    JOIN animal a  ON a.identifiant          = ad.ani_identifiant
    JOIN contact c ON c.contact_identifiant  = ad.adop_contact
    WHERE ad.adoption_id = p_adoption_id;

END;
$$;

alter function public.adoption_afficher_un(integer) owner to postgres;

create or replace function public.adoption_afficher_par_animal(p_identifiant_animal character varying)
    returns TABLE(adoption_id integer, statut character varying, date_demande date, ani_identifiant character varying, ani_nom character varying, adop_contact integer, contact_nom character varying, contact_prenom character varying)
    language plpgsql
as
$$
DECLARE
    existe_animal animal%ROWTYPE;
BEGIN
    -- Vérifier que l''animal existe
    SELECT *
    INTO existe_animal
    FROM animal
    WHERE identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION
            'Animal % introuvable.',
            p_identifiant_animal;
    END IF;

    -- Retourner toutes les adoptions liées à cet animal avec les informations du contact
    -- La left join est pertinente ici car si l''animal n''a aucune adoption la jointure
    -- ne retournera rien alors que l''animal existe
    RETURN QUERY
    SELECT
        ad.adoption_id,
        ad.statut,
        ad.date_demande,
        ad.ani_identifiant,
        a.nom    AS ani_nom,
        ad.adop_contact,
        c.nom    AS contact_nom,
        c.prenom AS contact_prenom
    FROM adoption ad
    JOIN animal a  ON a.identifiant         = ad.ani_identifiant
    JOIN contact c ON c.contact_identifiant = ad.adop_contact
    WHERE ad.ani_identifiant = p_identifiant_animal
    ORDER BY ad.date_demande DESC;

END;
$$;

alter function public.adoption_afficher_par_animal(varchar) owner to postgres;

