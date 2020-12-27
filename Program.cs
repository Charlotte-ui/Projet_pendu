
using System;
using System.Collections.Generic;
using System.IO ;
using System.Linq;
using System.Collections;
using System.Threading;

namespace Projet_pendu
{
    class Program
    {

        // Constantes de nomenclature, utiles pour la lisibilité du programme
        static bool SIMULATION = false;
        public const bool CHOIX_MOT = true;
        public const bool DEVINE = false;
        public const string ABANDON = "1";
        public const string REGLES = "2";
        public const string AIDE = "3";

        // Constantes numériques
        public const int MAX_PENDU = 6 ;
        public const int TEMPS_ATTENTE = 2000 ;

        // Références d'adresses
        public const string ADRESSE_DICO = "dicoFR.txt" ;
        public const string ADRESSE_REGLES = "regles.txt" ;
        public const string ADRESSE_NIVEAUX = "niveaux.txt" ;
        public const string ADRESSE_DESSIN = "dessin.txt" ;
        public const string ADRESSE_HEURISTIQUE = "heuristiques.txt" ;

        // Dictionnaires
        public static List<string> dictionnaire;
        public static List<string> dictionnaireNiv0;
        public static List<string> dictionnaireNiv1;
        public static List<string> dictionnaireNiv2;
        public static List<string> dictionnaireNiv3;
        public static List<string> dictionnaireCourant;


        public struct Joueur {
            public string nom;
            public bool robot;
            public int nbVictoire;
            public bool role ; //true choisit mot, false devine
            public bool aInitialiser ; // quand on change de mot de jeu
            public int niv ; // niveau de difficulté pour le robot
        }

        public static string JoueCoup (ref Joueur j,List<string> lettresDejaJouees, char[] lettresDecouvertes, int niveau) {            
            string reponse="";
            string aide=(niveau<2)?", [3] pour recevoir une aide intelligente de l'ordinateur":"";
            if (j.robot){
                if (! SIMULATION) {
                    Console.WriteLine("C'est à {0} de deviner le mot.",j.nom);
                    Thread.Sleep(TEMPS_ATTENTE);
                }

                switch (j.niv) {
                    case 1 : return CoupAleatoire (lettresDejaJouees);
                    case 2 : return HeuristiqueMotCompatible(lettresDejaJouees,lettresDecouvertes, out List<string> motCompatibles);
                    case 3 : return HeuristiqueProbabiliste(lettresDejaJouees,dictionnaireCourant);
                    case 4 : return HeuristiqueCombinee(lettresDejaJouees,lettresDecouvertes);
                }  
            }
            else {
                Console.WriteLine("{0}, quelle lettre ou mot proposez vous ? (entrer [1] pour abandonner, [2] pour afficher les règles {1}) ", j.nom,aide);
                reponse = Console.ReadLine().ToUpper();
               
                while (( (niveau<2 && lettresDejaJouees.Contains(reponse)) || !IsChaineLegal(reponse) || reponse.Equals(REGLES)) && !reponse.Equals(ABANDON) && !(reponse.Equals(AIDE) && niveau<2)){
                    if (!IsChaineLegal(reponse) && !reponse.Equals(REGLES))  Console.WriteLine("Vous avez saisi un caractères non autorisé, veuillez recommencer.");
                    else if (niveau<2 && lettresDejaJouees.Contains(reponse)) Console.WriteLine("Cette lettre a déjà été jouée, choisissez en une autre.");
                    else if (reponse.Equals(REGLES)) {
                        AfficheRegles(ADRESSE_REGLES);
                        Console.WriteLine("{0}, quelle lettre ou mot proposez vous ? (entrer [1] pour abandonner, [2] pour afficher les règles {1})", j.nom,aide);
                    }
                    reponse = Console.ReadLine().ToUpper();    
                           
                }
                if (lettresDejaJouees.Contains(reponse)) reponse=""; // si une lettre a déjà été ajouté, et qu'on est en niveau 2 ou 3, alors elle compte comme une erreure
                else if (reponse.Length==1 && !reponse.Equals(AIDE)) lettresDejaJouees.Add(reponse) ;
            }
            return reponse;
        }

        public static string CoupAleatoire (List<string> lettresDejaJouees){    
            string reponse;
            do {
                int i = new Random().Next(65, 91);
                char c = (char) i;
                reponse = c.ToString();
            }
            while (lettresDejaJouees.Contains(reponse));
            lettresDejaJouees.Add(reponse);
            return reponse;
        }
      
        public static string HeuristiqueCombinee (List<string> lettresDejaJouees, char[] lettresDecouvertes){    
            HeuristiqueMotCompatible (lettresDejaJouees, lettresDecouvertes, out List<string> motCompatibles);
            return HeuristiqueProbabiliste (lettresDejaJouees, motCompatibles);
        }

        public static string HeuristiqueProbabiliste (List<string> lettresDejaJouees, List<string> mots){
            Dictionary<char,int> lettresPriorisees = new Dictionary<char, int>();
            int prioriteMax=1;
            char lettreLaPlusPrioritaire='?';

            foreach(string mot in mots){
                foreach (char lettre in mot)
                {
                    if (!lettresDejaJouees.Contains(lettre.ToString())){
                        if (lettresPriorisees.ContainsKey(lettre)){
                            lettresPriorisees[lettre]++;
                            if (lettresPriorisees[lettre]>prioriteMax){
                               prioriteMax=lettresPriorisees[lettre] ;
                               lettreLaPlusPrioritaire=lettre;
                            }
                        }
                        else {
                            lettresPriorisees.Add(lettre,1);
                            if (lettreLaPlusPrioritaire=='?') lettreLaPlusPrioritaire=lettre;
                        }
                    }

                }
            }
            lettresDejaJouees.Add(lettreLaPlusPrioritaire.ToString());
            return lettreLaPlusPrioritaire.ToString();
        }

        public static string HeuristiqueMotCompatible (List<string> lettresDejaJouees, char[] lettresDecouvertes, out List<string> motCompatibles){
            List<char> lettresAbsentes = new List<char>();
            motCompatibles = new List<string>();
            List<char> lettresCompatibles = new List<char>();

            foreach (string s in lettresDejaJouees)
            {
                if(!lettresDecouvertes.Contains(Char.Parse(s))) lettresAbsentes.Add(Char.Parse(s));
            }

            foreach(string mot in dictionnaireCourant){
                if (IsCompatible(mot,lettresDecouvertes,lettresAbsentes)){
                    motCompatibles.Add(mot);
                    foreach (char lettre in mot) {
                        if (!lettresCompatibles.Contains(lettre)) lettresCompatibles.Add(lettre);
                    }
                }
            }

            if (motCompatibles.Count()==1) return motCompatibles[0];

            int i = new Random().Next(0, lettresCompatibles.Count());
            char c = lettresCompatibles[i];
            return i.ToString();
        }

        private static bool IsCompatible (string mot, char[] lettresDecouvertes,List<char> lettresAbsentes){
            if (mot.Length != lettresDecouvertes.Length) return false;
            for (int i=0; i<mot.Length;i++) {
                if (lettresDecouvertes[i]!='_' && lettresDecouvertes[i]!=mot[i]) return false;
                if (lettresAbsentes.Contains(mot[i])) return false;
            }
            return true;
        }

        // verifie que la chaine ne contient pas de caractères non autorisées (chiffres ...)
        public static bool IsChaineLegal (string s){
            bool retour = true;
            foreach (char lettre in s){
                if ((int) lettre <65 || (int) lettre >90) retour = false;
            }
    
            return retour;
        } 

        public static bool IsLettreDansMot (char lettre, char[] mot, char[] lettresDecouvertes){
            bool res=false;
            for (int i=0; i<mot.Length;i++){
                if (mot[i]==lettre) {
                    lettresDecouvertes[i]=lettre;
                    res=true;
                }
            }
            return res;
        }

        public static bool TestEgaliteTableau (char[] tab1, char[] tab2) {
            if (tab1.Length != tab2.Length ) return false;
            for (int i=0; i<tab1.Length;i++){
                if (tab1[i]!=tab2[i]) return false;
            }
            return true;
        }

        public static List<string> ChargeFichier (string adresse) {
            List<string> l = new List<string>();
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

        public static void ChoixMot (ref Joueur j, out char[] mot, out char[] lettresDecouvertes){
	        int indexDico;
            bool motAccepte =false;
            string reponse="1";

            Console.WriteLine("C'est à {0} de choisir le mot.",j.nom);
			Random rndIndex = new Random();
			if(j.robot == true){
                if (!SIMULATION) Thread.Sleep(TEMPS_ATTENTE);
				indexDico = rndIndex.Next(0, dictionnaireCourant.Count);
				mot = dictionnaireCourant[indexDico].ToCharArray();
			}
			else{
                while (!motAccepte) {
                    while (reponse.Equals("1")){
                        Console.WriteLine("Choisissez un mot parmi la liste. Taper [1] pour afficher la liste");
                        reponse = Console.ReadLine();
                        if (reponse.Equals("1")) AfficheDico(dictionnaireCourant,200);
                    }
                    reponse=reponse.ToUpper();
                    motAccepte = dictionnaireCourant.Contains(reponse) && IsChaineLegal(reponse);
                    if (!motAccepte) {
                        Console.WriteLine("Le mot est introuvable sur le dictionnaire. Veuillez réessayer.");
                        reponse = Console.ReadLine();
                    }
				}
                mot = reponse.ToCharArray();
            }
			lettresDecouvertes = new char [mot.Length] ;
            for (int i = 0; i < mot.Length; i++)
            {
			    lettresDecouvertes[i] = '_';
                if (mot[i]=='-')lettresDecouvertes[i] = '-';
            }
                 
        }

        public static void AfficheTab (char[] tab){
            foreach(char lettre in tab){
                Console.Write(lettre);
                if (lettre=='_') Console.Write(" ");
            }
            Console.WriteLine();
        }

        public static void AfficheListe (List<string> l, int limite, int deb){
            if (l.Count<limite) limite=l.Count;
            if (deb<0) deb=0;
            for (;deb<limite;deb++){
                Console.Write(l[deb]);
                Console.Write(" ");
            }
            Console.WriteLine();
        }

        public static void AfficheDico (List<string> l, int limite){
            bool continu=true;
            int i=0;
            while (continu){
                AfficheListe (l, limite,i);
                Console.WriteLine("Afficher les 200 mots suivants ? [true/false]");
                while (!bool.TryParse(Console.ReadLine(),out continu)){
                    Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                }
                i+=200;
                limite+=200;
            }
        }

        public static void AfficheRegles (string adresse){
            List<string> regles=ChargeFichier(adresse);
            Console.WriteLine();
            CentrerLeTexte("~ ~ ~");
            foreach (string ligne in regles){
                CentrerLeTexte(ligne);
            }
            CentrerLeTexte("~ ~ ~");
            Console.WriteLine();
        }
        public static void DessinePendu (int taille, List<string> dessin){
           // Console.Clear();
            int i=0;
            while (!dessin[i].Equals(taille.ToString())) i++;
            for (int j=1 ; j<=7 ; j++ ) {
                CentrerLeTexte(dessin[i+j]);
            }       
        }
        private static void CentrerLeTexte(string texte){
            int nbEspaces = (Console.WindowWidth - texte.Length) / 2;
            if (nbEspaces>0) Console.SetCursorPosition(nbEspaces, Console.CursorTop);
            Console.WriteLine(texte);
        }

        public static void DemandeNom (ref string nom, string message){
            Console.WriteLine("Quelle est le nom {0} ?",message);
            nom= Console.ReadLine();
            Console.WriteLine("Bienvenu {0} !",nom);

        }

        public static void InitialisationJoueur (ref Joueur j1, ref Joueur j2, ref int n){
            int choixModeJeu;

            if (j1.aInitialiser && j2.aInitialiser){
                Console.WriteLine("Vous désirez jouer avec : deux ordinateurs [1], deux humains [2], un ordinateur contre un humain [3] ?");
                while (!int.TryParse(Console.ReadLine(),out choixModeJeu) ||  choixModeJeu<1 ||  choixModeJeu>3 ){
                    Console.WriteLine("Valeur erronée, veuillez entrer un entier 1, 2 ou 3 en fonction du mode de jeu désiré.");
                } 
            }

            else {
                Console.WriteLine("Le nouveau joueur sera : un ordinateur [1], un humains [2] ?");
                while (!int.TryParse(Console.ReadLine(),out choixModeJeu) ||  choixModeJeu<1 ||  choixModeJeu>2 ){
                    Console.WriteLine("Valeur erronée, veuillez entrer un entier 1 ou 2 en fonction du mode de jeu désiré.");
                } 
            }
            
            switch (choixModeJeu) {
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
            
            if (j1.aInitialiser && j2.aInitialiser){
                j1.role = (new Random().Next(0, 2) ==0)? CHOIX_MOT: DEVINE;
                j2.role = !j1.role;
            }
            
            j1.aInitialiser=false;
            j2.aInitialiser=false;

            if (j1.robot && j2.robot){
                Console.WriteLine("Les deux joueurs sont des robots.");

                Console.WriteLine("Choisissez une première heuristique pour {0}: aléatoire [1], par compatibilité de mots [2], par probabilité de lettres [3], les deux dernières combinées [4] (entrer [0] pour afficher le descriptif des heuristiques)",j1.nom);
                    while (!int.TryParse(Console.ReadLine(),out choixModeJeu) ||  choixModeJeu<1 || choixModeJeu>4 ){
                        if (choixModeJeu==0) AfficheRegles(ADRESSE_HEURISTIQUE);
                        else Console.WriteLine("Valeur erronée, veuillez entrer les chiffres 0,1,2,3 ou 4 uniquement.");
                    }
                j1.niv=choixModeJeu;

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

                if (choixModeJeu==2) {
                    SIMULATION=true;
                    Console.WriteLine("Choisissez un nombre n d'itérations du programmes :");
                    while (!int.TryParse(Console.ReadLine(),out n) ||  n<1 ){
                        Console.WriteLine("Valeur erronée, veuillez entrer un entier supérieur à 0.");
                    }
                }
                else SIMULATION=false;
            }
            else SIMULATION=false;
        }

        public static void InitialisationDictionnaires (){
            dictionnaire=ChargeFichier(ADRESSE_DICO);
            
            dictionnaireNiv0= new List<string>();
            dictionnaireNiv1= new List<string>();
            dictionnaireNiv2= new List<string>();
            dictionnaireNiv3= new List<string>();

            ModuleLongueurDuMot(dictionnaire,5,0,dictionnaireNiv0);
            ModuleLongueurDuMot(dictionnaire,7,1,dictionnaireNiv1);
            ModuleLongueurDuMot(dictionnaire,9,2,dictionnaireNiv2);
            ModuleLongueurDuMot(dictionnaire,5,3,dictionnaireNiv3);
        }

        public static void Score (Joueur j1, Joueur j2){
            Console.WriteLine("Fin de partie : \n score {0} : {1} \n score {2} : {3} ",j1.nom,j1.nbVictoire,j2.nom,j2.nbVictoire);
        }

        public static void ChoixNiveau(ref int niv, ref Joueur j1, ref Joueur j2){
            
            Console.WriteLine("Choississez un niveau de difficulté [0,1,2,3]. Entrer -1 pour afficher le descriptifs des niveaux");
            while (!int.TryParse(Console.ReadLine(),out niv) || !(niv==0 || niv==1 || niv==2 || niv==3)){
                if (niv != -1) Console.WriteLine("Valeur erronée, veuillez entrer 1,2,3 ou -1.");
                else {
                    AfficheRegles(ADRESSE_NIVEAUX);
                    Console.WriteLine("Choississez un niveau de difficulté [0,1,2,3].");
                }
            }

            switch (niv){
                case 0:
                dictionnaireCourant=dictionnaireNiv0;
                break;
                case 1 :
                dictionnaireCourant=dictionnaireNiv1;
                break;
                case 2 :
                dictionnaireCourant=dictionnaireNiv2;
                break;
                case 3:
                dictionnaireCourant=dictionnaireNiv3;
                break;
            }

            if (!(j1.robot && j2.robot)) {
                j1.niv=niv;
                j2.niv=niv;
            }

        }

        public static void ChangementModeJeu (ref Joueur j1, ref Joueur j2, ref int n){
            int choixModeJeu;
            Console.WriteLine("Voulez-vous changer le premier joueur [1], le second joueur [2], les deux [3] ?");
            while (!int.TryParse(Console.ReadLine(),out choixModeJeu)){
                Console.WriteLine("Valeur erronée, veuillez entrer 1, 2 ou 3.");
            }
            if (choixModeJeu== 1) {
                Console.WriteLine("Aurevoir {0} ! Votre score était de {1}.",j1.nom,j1.nbVictoire);
                j1.nbVictoire=0;
                j1.aInitialiser=true;
                InitialisationJoueur (ref j1,ref j2, ref n);
            }
            if (choixModeJeu== 2) {
                Console.WriteLine("Aurevoir {0} ! Votre score était de {1}.",j2.nom,j2.nbVictoire);
                j2.aInitialiser=true;
                InitialisationJoueur (ref j1,ref j2, ref n);
            }
            if (choixModeJeu== 3) {
                Score( j1, j2);
                j1.aInitialiser=true;
                j2.aInitialiser=true;
                InitialisationJoueur (ref j1,ref j2, ref n);
            }
        }

        public static void ModuleLongueurDuMot(List <string> l, uint longueurMot, uint modeDeDifficulte, List<string> motsParTaille){
		foreach (string s in l)
            {
				if(modeDeDifficulte < 3){
					if(s.Length <= longueurMot) motsParTaille.Add(s);
				}
				else 
					if(s.Length >= longueurMot) motsParTaille.Add(s);
            }
	 
        }

        public static void AfficheInfo (int taillePendu, char[] lettresDecouvertes, List<string> lettresDejaJouees, List<string> dessin){
            DessinePendu(taillePendu,dessin);
            AfficheTab(lettresDecouvertes);
            Console.Write("\nLettres déjà jouées : ");
            AfficheListe(lettresDejaJouees,27,0);
        }

        public static void InitialisationProgramme (ref Joueur j1,ref Joueur j2, ref int niveau, ref int n){
            j1.aInitialiser=true;
            j2.aInitialiser=true;
            InitialisationJoueur (ref j1,ref j2, ref n);
            InitialisationDictionnaires ();
            ChoixNiveau(ref niveau, ref j1, ref j2);
        }

        public static void MessageDeFin (bool perdu, int taillePendu, char[] mot, ref Joueur j1, ref Joueur j2, ref bool continuerAJouer, ref int niveau, ref int n, List<string> dessin){
            bool changement;
            if (perdu){
                    DessinePendu(taillePendu,dessin);
                    Console.WriteLine ("{0}, vous avez perdu ! Le mot a deviner était :",(j1.role==DEVINE)?j1.nom:j2.nom);
                    AfficheTab(mot);
                }
                else {
                    Console.WriteLine ("{0}, vous avez gagné ! Le mot a deviner était :",(j1.role==DEVINE)?j1.nom:j2.nom);
                    AfficheTab(mot);
                    if (j1.role==DEVINE) j1.nbVictoire++;
                    else j2.nbVictoire++;
                }


                Console.WriteLine(" Voulez-vous faire une nouvelle partie [true/false] ?");
                while (!bool.TryParse(Console.ReadLine(),out continuerAJouer)){
                    Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                }


                if (continuerAJouer){
                    Console.WriteLine(" Voulez-vous changer de mode de jeu [true/false] ?");
                    while (!bool.TryParse(Console.ReadLine(),out changement)){
                        Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                    }
                    if (changement) ChangementModeJeu (ref j1, ref j2, ref n);


                    Console.WriteLine("Voulez-vous changer de niveau [true/false] ?");
                    while (!bool.TryParse(Console.ReadLine(),out changement)){
                        Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                    }
                    if (changement) ChoixNiveau(ref niveau, ref j1, ref j2);
                }

        }

        static void Main(string[] args)
        {
            int taillePendu=0;
            int niveau=0;
            int n=1;
            int nbIteration=0;
            bool continuerAJouer=true;
            bool perdu = false;
            string coup;
            char [] mot, lettresDecouvertes;
            Joueur j1 = new Joueur();
            Joueur j2 = new Joueur();
            List<string> lettresDejaJouees = new List<string>();
            List<string> dessin = ChargeFichier(ADRESSE_DESSIN);
            
            InitialisationProgramme (ref j1,ref j2,ref niveau, ref n);
            
           

            while ((continuerAJouer && !SIMULATION) || (SIMULATION && nbIteration<n)){
                Console.Clear();
                // choix du mot à faire deviner
                if (j1.role==CHOIX_MOT) ChoixMot(ref j1,out mot, out lettresDecouvertes);
                else                    ChoixMot(ref j2,out mot, out lettresDecouvertes);

                // l'autre joueur tente de deviner avec max 6 erreurs
                while (!(perdu || TestEgaliteTableau(mot,lettresDecouvertes))){
                 
                    if (!SIMULATION)  AfficheInfo ( taillePendu, lettresDecouvertes, lettresDejaJouees,dessin);

                    if (j1.role==DEVINE)  {
                        coup=JoueCoup(ref j1,lettresDejaJouees,lettresDecouvertes, niveau);
                        if (!SIMULATION && j1.robot) Console.WriteLine("{0} joue le coup \"{1}\"",j1.nom,coup);

                    }
                    else {
                        coup=JoueCoup(ref j2,lettresDejaJouees,lettresDecouvertes, niveau);
                        if (!SIMULATION && j2.robot) Console.WriteLine("{0} joue le coup \"{1}\"",j2.nom,coup);
                    }

                    if (coup.Equals(ABANDON)) perdu = true ;
                    if (coup.Equals(AIDE)) {
                        coup = HeuristiqueCombinee(lettresDejaJouees, lettresDecouvertes) ;
                        Console.WriteLine("L'ordinateur choisit pour vous la réponse \"{0}\"",coup);
                        if (coup.Length==1) lettresDejaJouees.Add(coup);
                        Thread.Sleep(TEMPS_ATTENTE);
                    }
                    
                    else if (coup.Length==1){
                        if (!IsLettreDansMot(char.Parse(coup), mot, lettresDecouvertes)){
                            taillePendu++;
                        }
                    }
                    else {
                        if (TestEgaliteTableau(coup.ToCharArray(),mot)){
                            lettresDecouvertes=mot;
                        }
                        else {
                            perdu=true;
                        }
                    }
                    if (taillePendu==MAX_PENDU) perdu=true;                         
                }
                
                if (!SIMULATION) MessageDeFin ( perdu,  taillePendu,  mot, ref  j1, ref  j2, ref  continuerAJouer, ref  niveau, ref n,dessin);                
                else if (!perdu){
                    if (j1.role==DEVINE) j1.nbVictoire++;
                    else j2.nbVictoire++;
                }
                
                j1.role=!j1.role;
                j2.role=!j2.role;

                //réinitialisation des variable
                lettresDejaJouees.Clear();
                dictionnaireCourant.Remove(new String(mot));
                perdu = false ;
                taillePendu=0;
                nbIteration++;
            }

            Score( j1, j2);
        }
    }
}
