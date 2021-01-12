using System;
using System.Collections.Generic;
using System.IO ;
using System.Linq;
using System.Threading;

namespace Projet_pendu
{
    /// <summary>
    /// La classe contenant le programme du jeu de pendu.
    /// Le jeu se joue en monde console avec une approche procédurale.
    /// </summary>
    /// <remarks>
    /// Trois types de modes de jeu existent : humain contre humain, humain contre ordinateur, ordinateur contre ordinateur.
    /// 4 niveaux de difficultés sont disponibles.
    /// L'IA de l'ordinateur dispose de 3 heuristiques de choix + un mode aléatoire.
    /// Dans le mode de jeu ordinateur contre ordinateur, deux alternatives sont possibles : démonstration (voir le programme se dérouler) ou simulation (comparer l'efficacité des heuristiques sur n itérations).
    /// </remarks>

    class Program
    {

        // Constantes de nomenclature, utiles pour la lisibilité du programme.
        /// <summary>
        /// Indique si le jeu est en mode simulation.
        /// </summary>
        /// <value>
        /// true ou false
        /// </value>
        static bool isSIMULATION = false;

        /// <summary>
        /// Rend plus lisible le rôle du joueur (choisir le mot ou le deviner).
        /// </summary>
        /// <value>
        /// true
        /// </value>
        public const bool CHOIX_MOT = true;
        
        /// <summary>
        /// Rend plus lisible le rôle du joueur (choisir le mot ou le deviner).
        /// </summary>
        /// <value>
        /// false
        /// </value>
        public const bool DEVINE = false;
        
        /// <summary>
        /// Rend plus lisible les coups spéciaux (abandonner, voir les règles, recevoir de l'aide).
        /// </summary>
        /// <value>
        /// "1"
        /// </value>
        public const string ABANDON = "1";
        
        /// <summary>
        /// Rend plus lisible les coups spéciaux (abandonner, voir les règles, recevoir de l'aide).
        /// </summary>
        /// <value>
        /// "2"
        /// </value>
        public const string REGLES = "2";
        
        /// <summary>
        /// Rend plus lisible les coups spéciaux (abandonner, voir les règles, recevoir de l'aide).
        /// </summary>
        /// <value>
        /// "3"
        /// </value>
        public const string AIDE = "3";

        // Constantes numériques
        
        /// <summary>
        /// Nombre maximum d'étapes avant la fin du pendu, à savoir le nombre d'erreurs pour échouer.
        /// </summary>
        /// <value>
        /// 6
        /// </value>
        public const int MAX_PENDU = 6 ;
        
        /// <summary>
        /// Limite de mots proposés à l'utilisateur à afficher.
        /// </summary>
        /// <value>
        /// 200
        /// </value>
        public const int MAX_MOTS = 200 ;

        /// <summary>
        /// Temps d'attente, en ms, après chacune des actions de l'ordinateur, pour que l'humain ait le temps de les lire.
        /// </summary>
        /// <value>
        /// 2000
        /// </value>
        public const int TEMPS_ATTENTE = 2000 ;
        /// <summary>
        /// Limite de taille du dictionnaire utilisé.
        /// </summary>
        /// /// <value>
        /// 75000
        /// </value>
        public const int LIMITE_DICOCOURANT = 75000 ;


        // Références d'adresses
        
        /// <summary>
        /// Adresse du dictionnaire principal.
        /// </summary>
        /// <value>
        /// "dicoFR.txt"
        /// </value>
        public const string ADRESSE_DICO = "dicoFR.txt" ;
        
        /// <summary>
        /// Adresse du descriptif des régles.
        /// </summary>
        /// <value>
        /// "regles.txt"
        /// </value>
        public const string ADRESSE_REGLES = "regles.txt" ;
        
        /// <summary>
        /// Adresse du descriptif des niveaux.
        /// </summary>
        /// <value>
        /// "niveaux.txt"
        /// </value>
        public const string ADRESSE_NIVEAUX = "niveaux.txt" ;
        
        /// <summary>
        /// Adresse du dessin des différentes étapes du pendu en affichage console.
        /// </summary>
        /// <value>
        /// "dessin.txt"
        /// </value>
        public const string ADRESSE_DESSIN = "dessin.txt" ;
        
        /// <summary>
        /// Adresse du descriptif des heuristiques.
        /// </summary>
        /// <value>
        /// "heuristiques.txt"
        /// </value>
        public const string ADRESSE_HEURISTIQUE = "heuristiques.txt" ;

        // Dictionnaires
        
        /// <summary>
        /// Liste de tous les mots du dictionnaire.
        /// </summary>
        public static List<string> dictionnaire;
        /// <summary>
        /// Dictionnaire utilisé lors de la partie, différent en fonction du niveau.
        /// </summary>
        public static List<string> dictionnaireCourant;

        /// <summary>
        /// Représentation d'un joueur.
        /// Comporte toute les informations concernant un joueur.
        /// </summary>
        public struct Joueur {
            /// <summary>
            /// Le joueur possède un nom.
            /// </summary>
            public string nom;
            /// <summary>
            /// Le joueur est un humain ou ordinateur.
            /// </summary>
            public bool robot;
            /// <summary>
            /// Le joueur possède un nombre de victoires.
            /// </summary>
            public int nbVictoire;
            /// <summary>
            /// Le joueur possède un role (devine ou choisi le mot).
            /// </summary>
            public bool role ; // true => choisi mot, false => devine
            /// <summary>
            /// Le joueur doit être ou non modifié.
            /// </summary>
            public bool aInitialiser ; // Quand on change de mot de jeu.
            /// <summary>
            /// Si c'est un ordinateur, le joueur possède un niveau d'heuristique (de 0 à 3).
            /// </summary>
            public int niv ; // Niveau de difficulté pour le robot.
        }

        
        /// <summary>
        /// Charge chaque ligne d'un fichier dont l'adresse est passée en paramètre dans une liste de string.
        /// </summary>
        /// <param name="adresse">adresse du fichier dans le projet</param>
        /// <returns>La liste de toutes les lignes du fichier.</returns>
        /// <exception cref="System.IO.IOException">A lever si le fichier ne peut pas s'ouvrir. </exception>        
        public static List<string> ChargeFichier (string adresse) {
            List<string> l = new List<string>(); // Liste contenant les lignes du fichier à charger.
            
            // Un flux est ouvert en lecture. Si c'est possible, chaque ligne du fichier est lue et ajouté à l. Sinon une exception est levée.
            try 
            { 
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("iso-8859-1");
                StreamReader monStreamReader = new StreamReader(adresse,encoding); 
                string mot = monStreamReader.ReadLine(); 
                while (mot != null) { 
                    l.Add(mot);
                    mot = monStreamReader.ReadLine();
                } 
                monStreamReader.Close(); 
            } 
            catch (Exception ex) 
            { 
                Console.Write("Une erreur est survenue au cours de la lecture :"); 
                Console.WriteLine(ex.Message); 
            }
            return l;
        }

    // Méthodes du jeu ------------------------------------------------------

        /// <summary>
        /// Permet à un joueur j de jouer un coup, c'est-à-dire de proposer une lettre ou un mot, en fonction des lettres qui ont déjà été jouées précédement. Le comportement de la méthode diffère en fonction de si le j est un humain ou un robot. Si j est un robot, son comportement dépendra du niveau n et des lettres déjà découvertes.
        /// </summary>
        /// <param name="j">joueur qui devine le mot</param>
        /// <param name="lettresDejaJouees">liste de lettres ayant déjà été proposées</param>
        /// <param name="lettresDecouvertes">tableaux avec les lettres découvertes aux bons endroits, '_' sinon</param>
        /// <param name="niveau">niveau de difficulté</param>
        /// <returns>La lettre ou le mot joué par j.</returns>
        public static string JoueCoup (ref Joueur j,List<string> lettresDejaJouees, char[] lettresDecouvertes, int niveau) {            
            string reponse=""; // La variable de retour.
            string aide=(niveau<2)?", [3] pour recevoir une aide intelligente de l'ordinateur":""; // En deçà d'un certain niveau, une aide intelligente est autorisée.
            if (j.robot){ // Cas ou j est un robot.
                if (! isSIMULATION) { // En mode jeu, un message explicatif est affiché.
                    Console.WriteLine("C'est à {0} de deviner le mot.",j.nom);
                    Thread.Sleep(TEMPS_ATTENTE);
                }

                switch (j.niv) { // Le robot joue différement en fonction du niveau, avec des heuristiques d'efficacité croissante.
                    case 1 : return CoupAleatoire (lettresDejaJouees);
                    case 2 : return HeuristiqueProbabiliste(lettresDejaJouees,dictionnaireCourant);
                    case 3 : return HeuristiqueMotCompatible(lettresDejaJouees,lettresDecouvertes, out List<string> motCompatibles);
                    case 4 : return HeuristiqueCombinee(lettresDejaJouees,lettresDecouvertes);
                }  
            }
            else { // Cas où j est humain.
                // On demande à l'utilisateur de proposer une réponse.
                Console.WriteLine("{0}, quelle lettre ou mot proposez vous ? (entrer [1] pour abandonner, [2] pour afficher les règles {1}) ", j.nom,aide);
                reponse = Console.ReadLine().ToUpper();
               
               // Tant que sa réponse contient des caractères invalides (voir IsChaineLegal) ou, en deçà d'un certain niveau, déjà joués précédement, on redemande une réponse à l'utilisateur.
                while (( (niveau<2 && lettresDejaJouees.Contains(reponse)) || !IsChaineLegal(reponse) || reponse.Equals(REGLES)) && !reponse.Equals(ABANDON) && !(reponse.Equals(AIDE) && niveau<2)){
                    if (!IsChaineLegal(reponse) && !reponse.Equals(REGLES))  Console.WriteLine("Vous avez saisi un caractères non autorisé, veuillez recommencer.");
                    else if (niveau<2 && lettresDejaJouees.Contains(reponse)) Console.WriteLine("Cette lettre a déjà été jouée, choisissez en une autre.");
                    else if (reponse.Equals(REGLES)) { // L'utilisateur peut demander à voir s'afficher les règles.
                        AfficheRegles(ADRESSE_REGLES);
                        Console.WriteLine("{0}, quelle lettre ou mot proposez vous ? (entrer [1] pour abandonner, [2] pour afficher les règles {1})", j.nom,aide);
                    }
                    reponse = Console.ReadLine().ToUpper(); // Tous les mots du dictionnaire étant en majuscules, la réponse de l'utilisateur est automatiquement mise en lettres capitales pour éviter les problèmes de casse.
                           
                }
                if (lettresDejaJouees.Contains(reponse)) reponse=""; // Si une lettre a déjà été ajoutée, et qu'on est en niveau 2 ou 3, alors elle compte comme une erreur (une chaine vide provoquera une erreur).
                else if (reponse.Length==1 && !reponse.Equals(AIDE)) lettresDejaJouees.Add(reponse) ; // Si l'utilisateur joue une lettre, elle est ajoutée à la liste des lettres déjà jouées.
            }
            return reponse;
        }

        /// <summary>
        /// Renvoie une lettre de l'alphabet aléatoire n'ayant pas déjà été jouée.
        /// </summary>
        /// <param name="lettresDejaJouees">liste de lettres ayant déjà été proposées</param>
        /// <returns>Lettre jouée.</returns>
        public static string CoupAleatoire (List<string> lettresDejaJouees){    
            string reponse; // Variable de retour.
            do { // On crée un entier aléatoire entre 65 et 97 (alphabet majuscule sur la table ASCII) qu'on transtype en char puis en string.
                int i = new Random().Next(65, 91); 
                char c = (char) i;
                reponse = c.ToString();
            }
            while (lettresDejaJouees.Contains(reponse)); // On recommence tant que la lettre à déjà été jouée.
            lettresDejaJouees.Add(reponse); // La lettre jouée est ajoutée à la liste des lettres déjà jouées.
            return reponse;
        }
      
        /// <summary>
        /// Renvoie une lettre ou un mot en appliquant l'HeuristiqueProbabiliste sur les résultats de l'HeuristiqueCompatible.
        /// </summary>
        /// <param name="lettresDejaJouees">liste de lettres ayant déjà été proposées</param>
        /// <param name="lettresDecouvertes">tableaux avec les lettres découvertes au bons endroits, '_' sinon</param>
        /// <returns>Lettre jouée.</returns>
        public static string HeuristiqueCombinee (List<string> lettresDejaJouees, char[] lettresDecouvertes){    
            HeuristiqueMotCompatible (lettresDejaJouees, lettresDecouvertes, out List<string> motCompatibles); //la liste de mot compatible est récupérée
            return HeuristiqueProbabiliste (lettresDejaJouees, motCompatibles);
        }

        /// <summary>
        /// Renvoie la lettre la plus souvent présente dans une liste de mots et qui n'est pas une lettre déjà jouée.
        /// </summary>
        /// <param name="lettresDejaJouees">liste de lettres ayant déjà été proposées</param>
        /// <param name="mots">liste de mots dans lesquels les lettres sont comptées</param>
        /// <returns>Lettre jouée.</returns>
        public static string HeuristiqueProbabiliste (List<string> lettresDejaJouees, List<string> mots){
            Dictionary<char,int> lettresPriorisees = new Dictionary<char, int>(); // Dictionnaire contenant chaque lettre et leur nombre d'occurences.
            int prioriteMax=1; // Occurence la plus élevée. 
            char lettreLaPlusPrioritaire='?'; // Lettre ayant la plus grande occurence.

            foreach(string mot in mots){ // Pour tous les mots de la liste.
                foreach (char lettre in mot) // Et pour chaque lettre de ces mots.
                {
                    if (!lettresDejaJouees.Contains(lettre.ToString())){ // Si la lettre n'a pas déjà été jouée...
                        if (lettresPriorisees.ContainsKey(lettre)){ //  ...On augmente son nombre d'occurence.
                            lettresPriorisees[lettre]++;
                            if (lettresPriorisees[lettre]>prioriteMax){ // Si son nombre d'occurence est supérieur au maximum...
                               prioriteMax=lettresPriorisees[lettre] ; // ...Elle devient la lettre prioritaire.
                               lettreLaPlusPrioritaire=lettre;
                            }
                        }
                        else { // Ou si elle n'est pas encore présente on l'ajoute avec une priorité de 1.
                            lettresPriorisees.Add(lettre,1); 
                            if (lettreLaPlusPrioritaire=='?') lettreLaPlusPrioritaire=lettre;
                        }
                    }

                }
            }
            lettresDejaJouees.Add(lettreLaPlusPrioritaire.ToString()); // La lettre prioritaire à la fin est renvoyée.
            return lettreLaPlusPrioritaire.ToString();
        }

        /// <summary>
        /// Renvoie une lettre au hasard parmis celles présentent dans les mots compatibles,  
        /// c'est-à-dire présentant les lettres déjà découvertent au bon endroit et ne comportant pas les lettres déjà jouées et rejetées.
        /// </summary>
        /// <param name="lettresDejaJouees">liste de lettres ayant déjà été proposées</param>
        /// <param name="lettresDecouvertes">tableaux avec les lettres découvertes aux bons endroits, '_' sinon</param>
        /// <param name="motCompatibles">mots du dictionnaire courant considérés comme compatibles</param>
        /// <returns>Lettre jouée.</returns>
        public static string HeuristiqueMotCompatible (List<string> lettresDejaJouees, char[] lettresDecouvertes, out List<string> motCompatibles){
            List<char> lettresAbsentes = new List<char>(); // Lettres déjà jouées mais qui ne font pas partie du mot.
            motCompatibles = new List<string>(); // Liste des mots compatibles.
            List<char> lettresCompatibles = new List<char>(); // Lettres des mots compatibles.

            // On remplit la liste des lettres absentes.
            foreach (string s in lettresDejaJouees)
            {
                if(!lettresDecouvertes.Contains(Char.Parse(s))) lettresAbsentes.Add(Char.Parse(s));
            }

            // On vérifie la compatibilité de chaque mot du dictionnaire et on récupère les lettres.
            foreach(string mot in dictionnaireCourant){
                if (IsCompatible(mot,lettresDecouvertes,lettresAbsentes)){
                    motCompatibles.Add(mot);
                    foreach (char lettre in mot) {
                        if (!lettresCompatibles.Contains(lettre)) lettresCompatibles.Add(lettre);
                    }
                }
            }

            if (motCompatibles.Count()==1) return motCompatibles[0]; // Si un seul mot est compatible il est renvoyé.

            int i = new Random().Next(0, lettresCompatibles.Count()); // Sinon il est renvoyé une lettre au hasard parmi la liste.
            char c = lettresCompatibles[i];
            return c.ToString();
        }
       
        /// <summary>
        /// Permet au joueur j de choisir un mot dans le dictionnaire courant. Il se comporte différemment en fonction de si j est un robot ou un humain. Le mot est initialisé comme une liste de char, et les lettres découvertes sont initialisées comme un tableau de même taille contenat uniquement les caractères - et _.
        /// </summary>
        /// <param name="j">joueur choisissant le mot</param>
        /// <param name="mot">mot choisi pour être deviné</param>
        /// <param name="lettresDecouvertes">tableau avec les lettres découvertes aux bons endroits, '_' sinon</param>
        public static void ChoixMot (ref Joueur j, out char[] mot, out char[] lettresDecouvertes){
	        int indexDico; // Index du mot dans le dico.
            bool motAccepte =false;
            string reponse="1"; // Variable de retour.
            Random rndIndex = new Random();

            if (!isSIMULATION) Console.WriteLine("C'est à {0} de choisir le mot.",j.nom);
			
			if(j.robot == true){ // Si j est un robot, on choisit un mot au hasard dans le dictionnaire.
                if (!isSIMULATION) Thread.Sleep(TEMPS_ATTENTE);
				indexDico = rndIndex.Next(0, dictionnaireCourant.Count);
				mot = dictionnaireCourant[indexDico].ToCharArray();
			}
			else{
                while (!motAccepte) { // Sinon, l'utilisateur choisit un mot dans le dictionnaire.
                    while (reponse.Equals("1")){
                        Console.WriteLine("Choisissez un mot parmi la liste. Taper [1] pour afficher la liste.");
                        reponse = Console.ReadLine();
                        if (reponse.Equals("1")) AfficheDico(dictionnaireCourant,MAX_MOTS); // L'utilisateur peut afficher la liste des mots du dictionnaire si besoin.
                    }
                    reponse=reponse.ToUpper(); // Pour éviter les problèmes de casse la réponse de l'utilisateur est mise en majuscule.
                    motAccepte = dictionnaireCourant.Contains(reponse) ; // Pour être valide, le mot doit faire partie du dictionnaire.
                    if (!motAccepte) {
                        Console.WriteLine("Le mot est introuvable sur le dictionnaire. Veuillez réessayer.");
                        reponse = Console.ReadLine();
                    }
				}
                mot = reponse.ToCharArray();
            }

            // Après le choix du mot, on initialise le tableau de lettres découvertes.
			lettresDecouvertes = new char [mot.Length] ;
            for (int i = 0; i < mot.Length; i++)
            {
			    lettresDecouvertes[i] = '_'; // Les lettres sont remplacées par le caractères _
                if (mot[i]=='-')lettresDecouvertes[i] = '-'; // Les tirets restent affichés.
            }
                 
        }

    // Test de vérification ------------------------------------------------------
        
        /// <summary>
        /// Vérifie si un mot comporte les lettres découvertes au bon endroit et ne comporte pas les lettres absentes.
        /// </summary>
        /// <param name="mot">mot choisi pour être deviné</param>
        /// <param name="lettresDecouvertes">tableau avec les lettres découvertes aux bons endroits, '_' sinon</param>
        /// <param name="lettresAbsentes">lettres ne pouvant pas être présentes dans le mot</param>
        /// <returns>Vrai si le mot est compatible, faux sinon.</returns>
        private static bool IsCompatible (string mot, char[] lettresDecouvertes,List<char> lettresAbsentes){
            if (mot.Length != lettresDecouvertes.Length) return false; // Si le mot n'est pas de la bonne taille il est rejeté.
            for (int i=0; i<mot.Length;i++) { // Sinon on vérifie chaque lettre.
                if (lettresDecouvertes[i]!='_' && lettresDecouvertes[i]!=mot[i]) return false; // Si l'emplacement découvert n'est pas vide, il doit correspondre.
                if (lettresAbsentes.Contains(mot[i])) return false; // La lettre ne doit pas déjà avoir été rejetée.
            }
            return true; // Si le mot passe tous les tests, il est accepté.
        }

        //  
        /// <summary>
        /// Verifie que la chaine s ne contient que des caractères autorisés, c'est-à-dire des lettres majuscules.
        /// </summary>
        /// <param name="s">chaine dont la légalité est à vérifier</param>
        /// <returns>Vrai si la chaine est légale, faux sinon.</returns>
        public static bool IsChaineLegal (string s){
            bool retour = true; // Variable de retour initialisée à true.
            foreach (char lettre in s){
                if ((int) lettre <65 || (int) lettre >90) retour = false; // Si une des lettres n'a pas le bon indice dans la table ASCII, le mot est rejeté.
            }
            return retour;
        } 

        /// <summary>
        /// Vérifie si une lettre est présente dans un mot. Si oui, le tableau des lettres découvertes est mis à jour.
        /// </summary>
        /// <param name="lettre">lettre dont on doit vérifier la présence</param>
        /// <param name="mot">mot à l'intérieur duquel on cherche la lettre</param>
        /// <param name="lettresDecouvertes">tableau avec les lettres découvertes aux bons endroits, '_' sinon</param>
        /// <returns>Vrai si la lettre est dans le mot, faux sinon.</returns>
        public static bool IsLettreDansMot (char lettre, char[] mot, char[] lettresDecouvertes){
            bool res=false; // Variable de retour initialisé à false.
            for (int i=0; i<mot.Length;i++){
                if (mot[i]==lettre) {
                    lettresDecouvertes[i]=lettre; // Si la lettre correspond, la lettre est acceptée et le tableau mis à jour.
                    res=true;
                }
            }
            return res;
        }

        /// <summary>
        /// Vérifie si le tableau tab1 comporte les mêmes valeur que le tableau tab2.
        /// </summary>
        /// <param name="tab1">premier tableau de char</param>
        /// <param name="tab2">second tableau de char</param>
        /// <returns>Vrai si les tableaux comportent les mêmes valeurs, faux sinon.</returns>
        public static bool TestEgaliteTableau (char[] tab1, char[] tab2) {
            if (tab1.Length != tab2.Length ) return false; // Si ils ne font pas la même taille alors ils sont différents.
            for (int i=0; i<tab1.Length;i++){
                if (tab1[i]!=tab2[i]) return false; // Si un caractère ne correspond pas, alors ils sont différents.
            }
            return true; // Sinon ils sont identiques.
        }

    // Méthodes d'affichage ------------------------------------------------------
        
        /// <summary>
        /// Affiche chaque lettre d'un tableau sur une ligne puis saute une ligne. 
        /// </summary>
        /// <param name="tab">tableau à afficher</param>
        public static void AfficheTab (char[] tab){
            foreach(char lettre in tab){
                Console.Write(lettre);
                if (lettre=='_') Console.Write(" "); // le caractère _ est suivit d'un espace pour plus de lisibilité
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Affiche une liste de mot sur une même ligne séparer par un espace, en commençant à l'indice deb et finissant à l'indice limite.
        /// </summary>
        /// <param name="l">liste de mot à afficher</param>
        /// <param name="limite">indice de fin</param>
        /// <param name="deb">indice de début</param>
        public static void AfficheListe (List<string> l, int limite, int deb){
            if (l.Count<limite) limite=l.Count; // si la limite dépasse la taille de la liste, elle est réduite
            if (deb<0) deb=0; // si l'indice de début est inférieur à 0, il est ramené à 0
            for (;deb<limite;deb++){
                Console.Write(l[deb]);
                Console.Write(" ");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Affiche les limites premiers mots de la liste l, puis demande à l'utilisateur s'il désire continuer l'affichage.
        /// </summary>
        /// <param name="l">liste des mots du dictionnaire</param>
        /// <param name="limite">nombre de mots à afficher</param>
        public static void AfficheDico (List<string> l, int limite){
            bool continu=true; // Continue l'affichage.
            int i=0; // Indice de départ.
            int iaffichage=limite;
            while (continu){
                AfficheListe (l, limite,i); // Affichage des limite premiers mots.
                Console.WriteLine("Afficher les {0} mots suivants ? [true/false]",iaffichage); // Demande à l'utilisateur s'il veut continuer.
                while (!bool.TryParse(Console.ReadLine(),out continu)){
                    Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                }
                i+=limite;
                limite+=limite;
            }
        }

        /// <summary>
        /// Affiche les règles du jeu stockées dans un fichier texte dont l'adresse est passé en paramètre.
        /// </summary>
        /// <param name="adresse">adresse du fichier dans le projet</param>
        public static void AfficheRegles (string adresse){
            List<string> regles=ChargeFichier(adresse); // Charge les lignes du fichier.
            Console.WriteLine();
            CentrerLeTexte("~ ~ ~");
            foreach (string ligne in regles){ // Affiche chaque ligne au milieu de l'écran.
                CentrerLeTexte(ligne);
            }
            CentrerLeTexte("~ ~ ~");
            Console.WriteLine();
        }
        
        /// <summary>
        /// Dessine le pendu contenu dans la liste dessin en fonction d'un certain niveau d'avancement déterminé par le paramètre taille (entre 0 et 6).
        /// </summary>
        /// <param name="taille">état d'avancement du pendu</param>
        /// <param name="dessin">liste de string constituant le dessin du pendu sur console</param>
        public static void DessinePendu (int taille, List<string> dessin){
            Console.Clear(); // Le pendu écrase le dessin précédent plutôt qu'il s'affiche à la suite.
            int i=0; // Indice du début du dessin dans la liste.
            while (!dessin[i].Equals(taille.ToString())) i++; // On cherche le chiffre correspondant à la taille avant de commencer l'affichage.
            for (int j=1 ; j<=7 ; j++ ) {
                CentrerLeTexte(dessin[i+j]); // On affiche les 7 lignes du pendu au milieu de l'écran.
            }       
        }
        
        /// <summary>
        /// Affiche une ligne de texte au milieu de la console.
        /// </summary>
        /// <param name="texte">texte a afficher au centre de la console</param>
        private static void CentrerLeTexte(string texte){
            int nbEspaces = (Console.WindowWidth - texte.Length) / 2; // Calcul de la position à laquelle mettre le curseur en fonction de la taille du mot.
            if (nbEspaces>0) Console.SetCursorPosition(nbEspaces, Console.CursorTop); // Si le texte à afficher est plus petit que la largeur de la console, le curseur est déplacé.
            Console.WriteLine(texte);
        }
        
        /// <summary>
        /// Demande son nom au joueur.
        /// </summary>
        /// <param name="nom">nom du joueur</param>
        /// <param name="message">message à afficher</param>
        public static void DemandeNom (ref string nom, string message){
            Console.WriteLine("Quelle est le nom {0} ?",message);
            nom= Console.ReadLine();
            Console.WriteLine("Bienvenu {0} !",nom);
        }
        
        /// <summary>
        /// Affiche les scores des joueurs j1 et j2.
        /// </summary>
        /// <param name="j1">premier joueur</param>
        /// <param name="j2">second joueur</param>
        public static void Score (Joueur j1, Joueur j2){
            Console.WriteLine("Fin de partie : \n score {0} : {1} \n score {2} : {3} ",j1.nom,j1.nbVictoire,j2.nom,j2.nbVictoire);
        }
        
        /// <summary>
        /// Affiche les informations nécessaires au joueur pour choisir un coup.
        /// </summary>
        /// <param name="taillePendu">état d'avancement du pendu</param>
        /// <param name="lettresDecouvertes">liste de lettre ayant déjà été proposées</param>
        /// <param name="lettresDejaJouees">liste de lettre ayant déjà été proposées</param>
        /// <param name="dessin">liste de string constituant le dessin du pendu sur console</param>
        public static void AfficheInfo (int taillePendu, char[] lettresDecouvertes, List<string> lettresDejaJouees, List<string> dessin){
            DessinePendu(taillePendu, dessin); // Dessine le pendu à la bonne taille.
            AfficheTab(lettresDecouvertes); // Affiche les lettres déjà découvertes.
            Console.Write("\nLettres déjà jouées : "); 
            AfficheListe(lettresDejaJouees,27,0); // Affiche la liste des lettres déjà jouées.
        }
    
    // Méthodes d'initialisation ------------------------------------------------------
        
        /// <summary>
        /// Initialise les paramètres des structures j1 et j2 ainsi que n, qui determine le nombre d'itérations en cas de simulation. 
        /// </summary>
        /// <param name="j1">premier joueur</param>
        /// <param name="j2">second joueur</param>
        /// <param name="n">nombre d'itérations</param>
        public static void InitialisationJoueur (ref Joueur j1, ref Joueur j2, ref int n){
            int choixModeJeu; // Variable permettant d'enregistrer les choix de l'utilisateur.

            if (j1.aInitialiser && j2.aInitialiser){ // Si les deux joueurs sont à initialiser, on propose trois options combinant ordinateurs et humains.
                Console.WriteLine("Vous désirez jouer avec : deux ordinateurs [1], deux humains [2], un ordinateur contre un humain [3] ?");
                while (!int.TryParse(Console.ReadLine(),out choixModeJeu) ||  choixModeJeu<1 ||  choixModeJeu>3 ){
                    Console.WriteLine("Valeur erronée, veuillez entrer un entier 1, 2 ou 3 en fonction du mode de jeu désiré.");
                } 
            }

            else { // Sinon, on propose deux possibilités (ordinateur ou humain) pour le joueur à initialiser.
                Console.WriteLine("Le nouveau joueur sera : un ordinateur [1], un humains [2] ?");
                while (!int.TryParse(Console.ReadLine(),out choixModeJeu) ||  choixModeJeu<1 ||  choixModeJeu>2 ){
                    Console.WriteLine("Valeur erronée, veuillez entrer un entier 1 ou 2 en fonction du mode de jeu désiré.");
                } 
            }
            
            // On initialise les noms et la nature (ordinateur ou humain) des joueurs.
            switch (choixModeJeu) { // Initialiser différemment en fonction de si l'utilisateur a demandé des ordinateurs ou des humains.
                case 1:
                    if (j1.aInitialiser) {
                        j1.nom="HAL";
                        j1.robot=true;
                    }
                    if (j2.aInitialiser) {
                        j2.nom="Skynet";
                        j2.robot=true;
                    }
                break;
                case 2:
                    if (j1.aInitialiser) {
                        DemandeNom(ref j1.nom,"du premier joueur");
                        j1.robot=false;
                    }
                    if (j2.aInitialiser) {
                        DemandeNom(ref j2.nom,"du second joueur");
                        j2.robot=false;
                    }
                break;
                case 3:
                    DemandeNom(ref j1.nom,"du premier joueur");
                    j2.nom="Skynet";
                    j1.robot=false;
                    j2.robot=true;
                break;
            }
            
            if (j1.aInitialiser && j2.aInitialiser){ // Si les deux joueurs sont nouveaux, on répartit les rôles aléatoirement.
                j1.role = (new Random().Next(0, 2) ==0)? CHOIX_MOT: DEVINE;
                j2.role = !j1.role;
            }
            
            j1.aInitialiser=false; // Les joueurs ne sont maintenant plus à initialiser.
            j2.aInitialiser=false;

            if (j1.robot && j2.robot){ // Si les deux joueurs sont des robots, l'utilisateur peut lancer une simulation ou une démonstration et choisir les heuristiques des robots.
                Console.WriteLine("Les deux joueurs sont des robots.");

                Console.WriteLine("Choisissez une première heuristique pour {0}: aléatoire [1], par probabilité de lettres [2], par compatibilité de mots [3], les deux dernières combinées [4] (entrer [0] pour afficher le descriptif des heuristiques)",j1.nom);
                    while (!int.TryParse(Console.ReadLine(),out choixModeJeu) ||  choixModeJeu<1 || choixModeJeu>4 ){
                        if (choixModeJeu==0) AfficheRegles(ADRESSE_HEURISTIQUE); // On affiche les descriptifs des heuristiques.
                        else Console.WriteLine("Valeur erronée, veuillez entrer les chiffres 0,1,2,3 ou 4 uniquement.");
                    }
                j1.niv=choixModeJeu; // On initialise le niveau du robot en fonction de l'heuristique choisi.

                Console.WriteLine("Choisissez une seconde heuristique pour {0} :",j2.nom);
                    while (!int.TryParse(Console.ReadLine(),out choixModeJeu) ||  choixModeJeu<1 || choixModeJeu>4 ){
                        if (choixModeJeu==0) AfficheRegles(ADRESSE_HEURISTIQUE);
                        else Console.WriteLine("Valeur erronée, veuillez entrer les chiffres 0,1,2,3 ou 4 uniquement.");
                    }
                j2.niv=choixModeJeu;

                Console.WriteLine("Voulez-vous lancer : [1] une démonstration (voir les deux robots s'affronter pas à pas) ou [2] une simulation (comparer les heuristiques sur n itérations du programmes) ?");
                while (!int.TryParse(Console.ReadLine(),out choixModeJeu) ||  choixModeJeu<1 ||  choixModeJeu>2 ){
                    Console.WriteLine("Valeur erronée, veuillez entrer un entier 1 ou 2 en fonction du mode de jeu désiré.");
                }

                if (choixModeJeu==2) { // Si l'utilisateur veut lancer une simulation, on lui demande de choisir un nombre d'itérations.
                    isSIMULATION=true;
                    Console.WriteLine("Choisissez un nombre n d'itérations du programme :");
                    while (!int.TryParse(Console.ReadLine(),out n) ||  n<1 || n>dictionnaireCourant.Count){
                        Console.WriteLine("Valeur erronée, veuillez entrer un entier supérieur à 0 et inférieur à {0} (taille di dictionnaire).", dictionnaireCourant.Count);
                    }
                }
                else isSIMULATION=false;
            }
            else isSIMULATION=false;
        }

        /// <summary>
        /// Initialisation des différents dictionnaires. Ceux qui servent à l'initialisation comme les intermédiaires.
        /// </summary>
        /// <param name="niv">niveau de difficulté</param>
        /// <param name="longueurMot">restriction sur la taille du mot</param>
        public static void InitialisationDictionnaire (int niv, int longueurMot){
            List<string> dictionnairePreTri= new List<string>();
            List<string> dictionnaireParTaille= new List<string>();
            List<string> dictionnaireParCommunRarete= new List<string>();
            List<string> dictionnaireParRepetition = new List<string>();
            dictionnaireCourant= new List<string>();
            dictionnaire=ChargeFichier(ADRESSE_DICO);
            InitialisationDictionnaireCourant(dictionnaire, dictionnairePreTri);
            ModuleLongueurDuMot(dictionnairePreTri, longueurMot, niv, dictionnaireParTaille); // application du module longueur du mot
            ModuleLettreCommunesRares(dictionnaireParTaille, niv, ref dictionnaireParCommunRarete); // application du module mot commun/rare
            ModuleRepetitionLettre(dictionnaireParCommunRarete, niv, dictionnaireParRepetition); // application du module mots répétés/non-répétés
            dictionnaireCourant = dictionnaireParRepetition;
        }

        /// <summary>
        /// Choix du niveau du jeu.
        /// </summary>
        /// <param name="niv">niveau de difficulté</param>
        /// <param name="j1">premier joueur</param>
        /// <param name="j2">second joueur</param>
        public static void ChoixNiveau(ref int niv, ref Joueur j1, ref Joueur j2){
            // l'utilisateur choisit une difficulté
            int[] tabLongueur = new int[] { 5, 7, 9, 7 }; //En fonction de la difficulté, un chiffre sera sauvegardé dans une variable
            Console.WriteLine("Choisissez un niveau de difficulté [0,1,2,3]. Entrer -1 pour afficher le descriptifs des niveaux");
            while (!int.TryParse(Console.ReadLine(),out niv) || !(niv==0 || niv==1 || niv==2 || niv==3)){
                if (niv != -1) Console.WriteLine("Valeur erronée, veuillez entrer 0,1,2,3 ou -1.");
                else {
                    AfficheRegles(ADRESSE_NIVEAUX); // l'utilisateur peut afficher le descriptif des niveaux
                    Console.WriteLine("Choisissez un niveau de difficulté [0,1,2,3].");
                }
            }
                int longueurMot = tabLongueur[niv]; //... Il servira pour plus tard à l'adaptation de la difficulté des mots en fonction de leur longueur.
                InitialisationDictionnaire (niv, longueurMot);
                

            if (!(j1.robot && j2.robot)) { // Si un seul des joueurs ou moins est un robot (mode jeu), son comportement dépend du niveau choisi.
                j1.niv=niv;
                j2.niv=niv;
            }
        }

        /// <summary>
        /// Permet à l'utilisateur de changer de mode de jeu (c'est à dire les caractéristiques des joueurs et le nombre d'itérations en mode simulation).
        /// </summary>
        /// <param name="j1">premier joueur</param>
        /// <param name="j2">second joueur</param>
        /// <param name="n">nombre d'itérations</param>
        public static void ChangementModeJeu (ref Joueur j1, ref Joueur j2, ref int n){
            int choixModeJeu; // Permet d'enregistrer le choix de l'utilisateur.
            Console.WriteLine("Voulez-vous changer le premier joueur [1], le second joueur [2], les deux [3] ?");
            while (!int.TryParse(Console.ReadLine(),out choixModeJeu)){
                Console.WriteLine("Valeur erronée, veuillez entrer 1, 2 ou 3.");
            }
            if (choixModeJeu== 1) { // Si un seul des joueurs est à initialiser, on lui dit aurevoir et lui affiche son score. L'autre joueur conserve son score pour la suivre.
                if (!isSIMULATION) Console.WriteLine("Aurevoir {0} ! Votre score était de {1}.",j1.nom,j1.nbVictoire);
                j1.nbVictoire=0;
                j1.aInitialiser=true;
            }
            if (choixModeJeu== 2) {
                if (!isSIMULATION) Console.WriteLine("Aurevoir {0} ! Votre score était de {1}.",j2.nom,j2.nbVictoire);
                j2.nbVictoire=0;
                j2.aInitialiser=true;
            }
            if (choixModeJeu== 3) { // Si les deux joueurs sont à changer, les scores des deux sont affichés et remis à 0.
                if (!isSIMULATION) Score( j1, j2);
                j1.nbVictoire=0;
                j2.nbVictoire=0;
                j1.aInitialiser=true;
                j2.aInitialiser=true;
            }
            InitialisationJoueur (ref j1,ref j2, ref n);
        }

        /// <summary>
        /// Récupère une quantité LIMITE_DICOCOURANT limitée de mots de la liste l dans dictionnaireCourantPreTri.
        /// </summary>
        /// <param name="l">liste totale des mots du dictionnaire</param>
        /// <param name="dictionnaireCourantPreTri">liste de LIMITE_DICOCOURANT mots extrait de ce dictionnaire</param>
        public static void InitialisationDictionnaireCourant(List <string> l, List<string> dictionnaireCourantPreTri){
            Random rndIndexDico = new Random();
            for(int i = 1; i <= LIMITE_DICOCOURANT; i++){ // Cette boucle s'effectue jusqu'à la limite du dictionnaire courant définie en constante.
                int index = rndIndexDico.Next(l.Count); // Génère un index d'une valeur aléatoire comprise entre 0 et le nombre d'éléments dans la liste du dictionnaire.
                dictionnaireCourantPreTri.Add(l[index]); // On ajoute l'élément aléatoire depuis le dictionnaire.
            }
        }

        /// <summary>
        /// Remplit la liste motsParTaille de mots de la liste l en fonction de leur taille et du niveau de difficulté.
        /// </summary>
        /// <param name="l">liste des mots du dictionnaire général</param>
        /// <param name="longueurMot">longueur limite du mot</param>
        /// <param name="modeDeDifficulte">niveau de difficulté</param>
        /// <param name="motsParTaille">liste de mots de taille conforme</param>
        public static void ModuleLongueurDuMot(List <string> l, int longueurMot, int modeDeDifficulte, List<string> motsParTaille){
		foreach (string s in l)
            {
				if(modeDeDifficulte < 3){ // Pour les bas niveau, les mots courts sont selectionnés.
                    if(s.Length <= longueurMot) motsParTaille.Add(s);
				}
				else // Pour les hauts niveaux, les mots longs sont selectionnés.
                    if(s.Length >= longueurMot) motsParTaille.Add(s);
            }
        }

        /// <summary>
        /// Remplit la liste listeTriee de mots de la liste l en fonction de la présence de lettres de la liste lettres.
        /// </summary>
        /// <param name="l">liste de référence</param>
        /// <param name="motsParCommunRarete">liste triée</param>
        public static void ContientListeLettre(List<string> l, List<string> listeTriee, List<string> lettres){
            bool ceMotCorrespond;
            foreach (String s in l) // Pour chaque mot de la liste.
            {
                ceMotCorrespond=false;
                for(int j = 0; j < lettres.Count && !ceMotCorrespond; j++){ // Pour chaque lettre commune, tant que le mot ne correspond pas.
                    if(s.Contains((string)lettres[j])){ // Si le mot contient une des lettres communes/rares.
                        if(!listeTriee.Contains(s))listeTriee.Add(s); // Et s'il n'est pas déjà dans la liste, on l'ajoute.
                        ceMotCorrespond=true; // On indique que le mot correspond.
                    }
                }   
            }
        }


        /// <summary>
        /// Remplit la liste motsParCommunRarete de mots de la liste l en fonction de la présence de mots rares, communs ou les deux.
        /// </summary>
        /// <param name="l">liste de mot initiale</param>
        /// <param name="modeDeDifficulte">niveau de difficulté</param>
        /// <param name="motsParCommunRarete">liste triée</param>
        public static void ModuleLettreCommunesRares(List<string> l, int modeDeDifficulte, ref List<string> motsParCommunRarete){
        List<string> lettresCommunes = new List<string>(){"R","S","T","L","N","E"}; 
        List<string> lettresRares = new List<string>(){"Z","Q","X","J"}; 
        if(modeDeDifficulte < 2){ // Pour les modes de difficultés 0 et 1 : Les mots ont obligatoirement une lettre commune.
            ContientListeLettre(l,motsParCommunRarete,lettresCommunes);
                        
        } else if(modeDeDifficulte == 2){ // Pour le mode difficulté 2 : Les mots ont obligatoirement une lettre commune et une lettre rare.
            List<string> tmp = new List<string>();
            ContientListeLettre(l,motsParCommunRarete,lettresCommunes);
            ContientListeLettre(motsParCommunRarete,tmp,lettresRares);
            motsParCommunRarete=tmp;              
        } else{ // Pour les modes de difficultés 3 : Les mots ont obligatoirement une lettre rare.
            ContientListeLettre(l,motsParCommunRarete,lettresRares);
        }  
        }

        /// <summary>
        /// Remplit la liste motsParRepetOuNon de mots de la liste l en fonction de la répétition ou non-répétition de lettres.
        /// </summary>
        /// <param name="l">liste initiale</param>
        /// <param name="modeDeDifficulte">niveau de difficulté</param>
        /// <param name="motsParCommunRarete">liste triée</param>
        public static void ModuleRepetitionLettre(List<string> l, int modeDeDifficulte, List<string> motsParRepetOuNon){ 
        
            foreach (String s in l){  // Pour chaque mot de la liste.
                bool lettreRepetee = false;
                int i = 0;
                var cSorted = new String(s.OrderBy(c => c).ToArray()); // Les lettres sont triées par ordre alphabétique...
                char lettreActuelle = ' ';
                char lettreSuivante = ' ';
                while(!lettreRepetee && i < s.Length-1){
                    lettreActuelle = cSorted[i]; // ... Pour ensuite comparer la lettre actuelle et la suivante entre elles...
                    lettreSuivante = cSorted[i+1];  
                    if(lettreActuelle == lettreSuivante){  // ... Et ainsi vérifier la répétition et ajouter le mot si la difficulté est basse.
                        if(modeDeDifficulte <= 1) if(!motsParRepetOuNon.Contains(s))motsParRepetOuNon.Add(s);
                        lettreRepetee = true;
                    }
                    i++;
                }
                if(modeDeDifficulte > 1 && !lettreRepetee) if(!motsParRepetOuNon.Contains(s))motsParRepetOuNon.Add(s); //Dans le cas où on a passé toutes les lettres sans jamais détecter de répétition, on ajoute le mot si la difficulté est forte.
            }
        }


        /// <summary>
        /// Initialise les variables nécessaires au démarage du programme (les joueurs, les dictionnaires et le niveau). 
        /// </summary>
        /// <param name="j1">premier joueur</param>
        /// <param name="j2">second joueur</param>
        /// <param name="niveau">niveau de difficulté</param>
        /// <param name="n">nombre d'itérations</param>
        public static void InitialisationProgramme (ref Joueur j1,ref Joueur j2, ref int niveau, ref int n){
            j1.aInitialiser=true;
            j2.aInitialiser=true;
            InitialisationJoueur (ref j1,ref j2, ref n); 
            ChoixNiveau(ref niveau, ref j1, ref j2);
        }

        /// <summary>
        /// Affiche un message de fin de partie et demande à l'utilisateur s'il veut continuer, changer de mode de jeu et de niveau.
        /// </summary>
        /// <param name="perdu">indique si la partie était perdante</param>
        /// <param name="mot">mot à deviner</param>
        /// <param name="j1">premier joueur</param>
        /// <param name="j2">second joueur</param>
        /// <param name="continuerAJouer">indique si l'utilisateur désire continuer à jouer</param>
        /// <param name="niveau">niveau de difficulté</param>
        /// <param name="n">nombre d'itération</param>
        /// <param name="dessin">liste de string constituant le dessin du pendu sur console</param>
        public static void MessageDeFin (bool perdu, char[] mot, ref Joueur j1, ref Joueur j2, ref bool continuerAJouer, ref int niveau, ref int n, List<string> dessin){
            bool changement;
            if (!isSIMULATION){ // En cas de simulation, les résultats finaux ne s'affichent pas à chaque itération, juste à la fin.
                if (perdu){
                    DessinePendu(MAX_PENDU,dessin);
                    Console.WriteLine ("{0}, vous avez perdu ! Le mot a deviner était :",(j1.role==DEVINE)?j1.nom:j2.nom);
                    AfficheTab(mot);
                }
                else {
                    Console.WriteLine ("{0}, vous avez gagné ! Le mot a deviner était :",(j1.role==DEVINE)?j1.nom:j2.nom);
                    AfficheTab(mot);
                    if (j1.role==DEVINE) j1.nbVictoire++;
                    else j2.nbVictoire++;
                }
            } else { // En cas de simulation, les scores sont réinitialisés après les n itérations.
                j1.nbVictoire=0;
                j2.nbVictoire=0;
            }
            
            Console.WriteLine(" Voulez-vous faire une nouvelle partie [true/false] ?"); // Demande à l'utilisateur s'il veut continuer à jouer.
            while (!bool.TryParse(Console.ReadLine(),out continuerAJouer)){
                Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
            }

            if (continuerAJouer){ // si l'utilisateur veut continuer 
                Console.WriteLine(" Voulez-vous changer de mode de jeu [true/false] ?"); // On lui demande s'il veut changer de mode de jeu.
                while (!bool.TryParse(Console.ReadLine(),out changement)){
                    Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                }
                if (changement) ChangementModeJeu (ref j1, ref j2, ref n);

                Console.WriteLine("Voulez-vous changer de niveau [true/false] ?"); // Et s'il veut changer de niveau.
                while (!bool.TryParse(Console.ReadLine(),out changement)){
                    Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                }
                if (changement) ChoixNiveau(ref niveau, ref j1, ref j2);
            }

        }

        static void Main(string[] args)
        {
            // Variables
            int taillePendu=0; // Niveau d'avancement du pendu et le nombre d'erreurs.
            int niveau=0; // Niveau de difficulté du jeu.
            int n=1; // Nombre maximal d'itérations du programme en mode simulation.
            int nbIteration=0; // Incrémentation comptant les itérations du programme.
            bool continuerAJouer=true; // Recommence une partie ou arrête le programme en fonction de l'utilisateur.
            bool perdu = false; // Défaite du joueur qui devine le mot.
            string coup; // Coup du joueur qui devine le mot (une lettre ou un mot).
            char [] mot, lettresDecouvertes; // Mot à deviner choisi par un joueur et les lettres correctement devinées par l'autre.
            Joueur j1 = new Joueur(); // Premier joueur.
            Joueur j2 = new Joueur(); // Second joueur.
            List<string> lettresDejaJouees = new List<string>(); // Lettres déjà jouées, vraies ou fausses.
            List<string> dessin = ChargeFichier(ADRESSE_DESSIN); // Dessin du pendu.
            
            InitialisationProgramme (ref j1,ref j2,ref niveau, ref n); // Initialise les joueurs (robot ou humain), le niveau et le nombre d'itérations maximal en cas de simulation.
            
            while (continuerAJouer){ // En cas de simulation, on sort de la boucle principale après n itératiosn du programme, il faut donc demander à l'utilisateur s'il veut continuer.
                while ((continuerAJouer && !isSIMULATION) || (isSIMULATION && nbIteration<n)){ // Boucle principale du programme, qui s'arrête en mode jeu si l'utilisateur le demande ou en mode simulation lorsque n est atteint.
                    Console.Clear(); // Pour davantage de visibilité on nettoie la console à chaque round.
                
                    // Choix du mot à faire deviner par j1 ou j2 en fonction de leur rôle.
                    if (j1.role==CHOIX_MOT) ChoixMot(ref j1,out mot, out lettresDecouvertes);
                    else                    ChoixMot(ref j2,out mot, out lettresDecouvertes);

                    while (!(perdu || TestEgaliteTableau(mot,lettresDecouvertes))){ // L'autre joueur tente de deviner le mot. On s'arrête au bout de 6 erreurs, d'une proposition de mot erronée ou d'une victoire.
                 
                        if (!isSIMULATION)  AfficheInfo ( taillePendu, lettresDecouvertes, lettresDejaJouees,dessin); // Affiche le dessin du pendu et les informations de jeu en mode jeu.

                        // Le joueur dont c'est le role cherche à deviner le mot.
                        if (j1.role==DEVINE)  {
                            coup=JoueCoup(ref j1,lettresDejaJouees,lettresDecouvertes, niveau);
                            if (!isSIMULATION && j1.robot) Console.WriteLine("{0} joue le coup \"{1}\"",j1.nom,coup); // En mode jeu, on affiche le coup joué par l'ordinateur.
                        }
                        else {
                            coup=JoueCoup(ref j2,lettresDejaJouees,lettresDecouvertes, niveau);
                            if (!isSIMULATION && j2.robot) Console.WriteLine("{0} joue le coup \"{1}\"",j2.nom,coup);
                        }

                        // Le joueur humain peut abandonner la partie.
                        if (coup.Equals(ABANDON)) perdu = true ;

                        // Le joueur humain peut demander de l'aide.
                        else if (coup.Equals(AIDE)) {
                            coup = HeuristiqueCombinee(lettresDejaJouees, lettresDecouvertes) ; // On fait appel à l'heuristique la plus efficace pour jouer à la place de l'humain.
                            Console.WriteLine("L'ordinateur choisi pour vous la réponse \"{0}\"",coup);
                            if (coup.Length==1) {
                                lettresDejaJouees.Add(coup);
                                IsLettreDansMot(char.Parse(coup), mot, lettresDecouvertes);
                            }
                            Thread.Sleep(TEMPS_ATTENTE); 
                        }
                    
                        // Si la taille du coup joué est de 1, alors c'est une lettre ; si elle n'est pas présente dans le mot à deviner, alors c'est une erreur.
                        else if (coup.Length==1) {
                            if (!IsLettreDansMot(char.Parse(coup), mot, lettresDecouvertes)) taillePendu++; 
                        }
                          
                        // Sinon c'est un mot ; si le mauvais mot est proposé, le joueur qui devine a perdu.
                        else {
                            if (TestEgaliteTableau(coup.ToCharArray(),mot)) lettresDecouvertes=mot;
                            else perdu=true;
                        }
         
                        if (taillePendu==MAX_PENDU) perdu=true;  // On perd si le pendu atteint sa taille maximale.
                    }
                
                    // En mode jeu, le message de fin s'affiche après chaque round.
                    if (!isSIMULATION) MessageDeFin ( perdu,  mot, ref  j1, ref  j2, ref  continuerAJouer, ref  niveau, ref n,dessin);                
                
                    // En cas de victoire, le nombre de victoires du joueur qui devine le mot s'incrémente.
                    else if (!perdu){
                        if (j1.role==DEVINE) j1.nbVictoire++;
                        else j2.nbVictoire++;
                    }
                
                    // Les rôles (choix du mot et devine) des deux joueurs sont échangés.
                    j1.role=!j1.role;
                    j2.role=!j2.role;

                    // Réinitialisation des variables et incrémentation du nombre d'itérations.
                    lettresDejaJouees.Clear();
                    dictionnaireCourant.Remove(new String(mot));
                    perdu = false ;
                    taillePendu=0;
                    nbIteration++;
                }

                // En mode simulation, les scores et le message de fin s'affichent après n itérations du programme.
                Score( j1, j2);
                if (isSIMULATION) MessageDeFin ( false,  null, ref  j1, ref  j2, ref  continuerAJouer, ref  niveau, ref n,dessin);
                nbIteration=0;   
            }
        }
    }
}
