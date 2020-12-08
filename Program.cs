
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
        public const bool VERBOSE = false;
        public const bool CHOIX_MOT = true;
        public const bool DEVINE = false;
        public const int MAX_PENDU = 5 ;
        public const int TEMPS_ATTENTE = 2000 ;
        public const string ADRESSE_DICO = "dicoFR.txt" ;
        public const string ADRESSE_REGLES = "regles.txt" ;
        public const string ABANDON = "1";
        public const string REGLES = "2";
        public const string AIDE = "3";
        public static List<string> dictionnaire;
        public static List<string> dictionnaireNiv0;
        public static List<string> dictionnaireNiv1;
        public static List<string> dictionnaireNiv2;
        public static List<string> dictionnaireNiv3;
        public static List<string> dictionnaireCourant;
        public static List<string> regles;


       public enum Fichier {
            dictionnaire,
            regles,
            dessin
        }


        public struct Joueur {
            public string nom;
            public bool robot;
            public int nbVictoire;
            public bool role ; //true choisit mot, false devine
            public bool aInitialiser ; // quand on change de mot de jeu
        }

        // reste le test de si contient des caractères non autorisés
        public static string JoueCoup (ref Joueur j,List<string> lettresDejaJouees, char[] lettresDecouvertes, int niveau) {            
            string reponse;
            string aide="";
            if (niveau<2) aide=", [3] pour recevoir une aide intelligente de l'ordinateur";
            
            if (j.robot){
                Console.WriteLine("C'est à {0} de deviner le mot.",j.nom);
                Thread.Sleep(TEMPS_ATTENTE);
                if (niveau<3) return CoupAleatoire (lettresDejaJouees);
                return coupIntelligent(lettresDejaJouees,lettresDecouvertes);
            }
            else {
                Console.WriteLine("{0}, quelle lettre ou mot proposez vous ? (entrer [1] pour abandonner, [2] pour afficher les règles {1}) ", j.nom,aide);
                reponse = Console.ReadLine().ToUpper();
               
                while (( (niveau<2 && lettresDejaJouees.Contains(reponse)) || !isChaineLegal(reponse) || reponse.Equals(REGLES)) && !reponse.Equals(ABANDON) && !(reponse.Equals(AIDE) && niveau<2)){
                    if (!isChaineLegal(reponse) && !reponse.Equals(REGLES))  Console.WriteLine("Vous avez saisi un caractères non autorisé, veuillez recommencer.");
                    else if (niveau<2 && lettresDejaJouees.Contains(reponse)) Console.WriteLine("Cette lettre a déjà été jouez, choisissez en une autre.");
                    else if (reponse.Equals(REGLES)) {
                        afficheRegles();
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
            Console.WriteLine(" joue la lettre {0}",reponse);
            lettresDejaJouees.Add(reponse);
            return reponse;
        }
      
        public static string coupIntelligent (List<string> lettresDejaJouees, char[] lettresDecouvertes){    
            List<char> lettresAbsentes = new List<char>();
            List<string> motCompatibles = new List<string>();

            foreach (string s in lettresDejaJouees)
            {
                if(!lettresDecouvertes.Contains(Char.Parse(s))) lettresAbsentes.Add(Char.Parse(s));
            }

            foreach(string mot in dictionnaireCourant){
                if (estCompatible(mot,lettresDecouvertes,lettresAbsentes)){
                    motCompatibles.Add(mot);
                }
            }

            if (motCompatibles.Count()==1) return motCompatibles[0];

            Dictionary<char,int> lettresPriorisees = new Dictionary<char, int>();
            int prioriteMax=1;
            char lettreLaPlusPrioritaire='?';

            foreach(string mot in motCompatibles){
                foreach (char lettre in mot)
                {
                    if (!lettresDecouvertes.Contains(lettre)){
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

        private static bool estCompatible (string mot, char[] lettresDecouvertes,List<char> lettresAbsentes){
            if (mot.Length != lettresDecouvertes.Length) return false;
            for (int i=0; i<mot.Length;i++) {
                if (lettresDecouvertes[i]!='_' && lettresDecouvertes[i]!=mot[i]) return false;
                if (lettresAbsentes.Contains(mot[i])) return false;
            }
            return true;
        }

        // verifie que la chaine ne contient pas de caractères non autorisées (chiffres ...)
        public static bool isChaineLegal (string s){
            bool retour = true;
            foreach (char lettre in s){
                if ((int) lettre <65 || (int) lettre >90) retour = false;
            }
    
            return retour;
        } 

        public static bool isLettreDansMot (char lettre, char[] mot, char[] lettresDecouvertes){
            if (VERBOSE) Console.WriteLine("entrée dans isLettreDansMot");
            bool res=false;
            for (int i=0; i<mot.Length;i++){
                if (mot[i]==lettre) {
                    lettresDecouvertes[i]=lettre;
                    res=true;
                }
            }
            return res;
        }

        public static bool deepEqualsTabChar (char[] tab1, char[] tab2) {
            if (tab1.Length != tab2.Length ) return false;
            for (int i=0; i<tab1.Length;i++){
                if (tab1[i]!=tab2[i]) return false;
            }
            return true;
        }

        public static void chargeFichier (string adresse, Fichier f) {
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

            switch (f){
                case (Fichier.dictionnaire) :
                dictionnaire = l;
                break;
                case (Fichier.regles) : 
                regles = l;
                break;
                case(Fichier.dessin):
                break;
            }




        }

        public static void choixMot (ref Joueur j, out char[] mot, out char[] lettresDecouvertes){
	        int indexDico;
            bool motAccepte =false;
            string reponse="1";

            //mot = new char[] {'C','H','A','T'};     
            //lettresDecouvertes = new char [] { '_', '_', '_','_','_'};   

            Console.WriteLine("C'est à {0} de choisir le mot.",j.nom);
			Random rndIndex = new Random();
			if(j.robot == true){
               Thread.Sleep(TEMPS_ATTENTE);
				indexDico = rndIndex.Next(0, dictionnaireCourant.Count);
				mot = dictionnaireCourant[indexDico].ToCharArray();
			}
			else{
                while (!motAccepte) {
                    while (reponse.Equals("1")){
                        Console.WriteLine("Choisissez un mot parmi la liste. Taper [1] pour afficher la liste");
                        reponse = Console.ReadLine();
                        if (reponse.Equals("1")) afficheDico(dictionnaireCourant,100);
                    }
                    reponse=reponse.ToUpper();
                    motAccepte = dictionnaireCourant.Contains(reponse) && isChaineLegal(reponse);
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

        public static void afficheTab (char[] tab){
            foreach(char lettre in tab){
                Console.Write(lettre);
                if (lettre=='_') Console.Write(" ");
            }
            Console.WriteLine();
        }

        public static void afficheListe (List<string> l, int limite, int deb){
            if (l.Count<limite) limite=l.Count;
            if (deb<0) deb=0;
            for (;deb<limite;deb++){
                Console.Write(l[deb]);
                Console.Write(" ");
            }
            Console.WriteLine();
        }

        public static void afficheDico (List<string> l, int limite){
            bool continu=true;
            int i=0;
            while (continu){
                afficheListe (l, limite,i);
                Console.WriteLine("Afficher les 100 mots suivants ? [true/false]");
                while (!bool.TryParse(Console.ReadLine(),out continu)){
                    Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                }
                i+=100;
                limite+=100;
            }
        }



        public static void afficheRegles (){
            Console.WriteLine();
            centrerLeTexte("~ ~ ~");
            foreach (string ligne in regles){
                centrerLeTexte(ligne);
            }
            centrerLeTexte("~ ~ ~");
            Console.WriteLine();
        }
        public static void dessinePendu (int taille){
            Console.Clear();
            centrerLeTexte(" _______");
            centrerLeTexte(" |/   | ");
            switch (taille) {
            case 0 :
                centrerLeTexte(" |      ");
                centrerLeTexte(" |      ");
                centrerLeTexte(" |      ");
                break;
            case 1 :
            centrerLeTexte(" |    O ");
            centrerLeTexte(" |      ");
            centrerLeTexte(" |      ");
            
            break;
            case 2 :
            centrerLeTexte(" |    O ");
            centrerLeTexte(" |   -| ");
            centrerLeTexte(" |      ");
            break;
            case 3 :
            centrerLeTexte(" |    O ");
            centrerLeTexte(" |   -|-");
            centrerLeTexte(" |      ");
            break;
            case 4 :
            centrerLeTexte(" |    O ");
            centrerLeTexte(" |   -|-");
            centrerLeTexte(" |    / ");
            break;
            case 5 :
            centrerLeTexte(" |    O ");
            centrerLeTexte(" |   -|-");
            centrerLeTexte(" |    /\\");
            break;
            }


            centrerLeTexte(" |      ");
            centrerLeTexte("-----------");




            
        }
        private static void centrerLeTexte(string texte){
            int nbEspaces = (Console.WindowWidth - texte.Length) / 2;
            if (nbEspaces>0) Console.SetCursorPosition(nbEspaces, Console.CursorTop);
            Console.WriteLine(texte);
        }

        public static void demandeNom (ref string nom, string message){
            Console.WriteLine("Quelle est le nom {0} ?",message);
            nom= Console.ReadLine();
        }

        public static void initialisationJoueur (ref Joueur j1, ref Joueur j2){
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
                    demandeNom(ref j1.nom,"du premier joueur");
                    j1.robot=false;
                }
                if (j2.aInitialiser) {
                    demandeNom(ref j2.nom,"du second joueur");
                    j2.robot=false;
                }
                break;
                case 3:
                demandeNom(ref j1.nom,"du premier joueur");
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
        }

        public static void initialisationDictionnaires (){
            dictionnaireNiv0= new List<string>(dictionnaire);
            dictionnaireNiv1= new List<string>(dictionnaire);
            dictionnaireNiv2= new List<string>(dictionnaire);
            dictionnaireNiv3= new List<string>(dictionnaire);
        }

        public static void messageFin (ref Joueur j1, ref Joueur j2){
            Console.WriteLine("Fin de partie \n score {0} : {1} \n score {2} : {3} ",j1.nom,j1.nbVictoire,j2.nom,j2.nbVictoire);
        }

        public static void ChoixNiveau(ref int niv){
            
            
            
            Console.WriteLine("Choississez un niveau de difficulté [0,1,2,3]. Entrer -1 pour afficher le descriptifs des niveaux");
            while (!int.TryParse(Console.ReadLine(),out niv) || !(niv==0 || niv==1 || niv==2 || niv==3)){
                if (niv != -1) Console.WriteLine("Valeur erronée, veuillez entrer 1,2,3 ou -1.");
                else Console.WriteLine("blabla");
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

        }

        public static void changementModeJeu (ref Joueur j1, ref Joueur j2){
            int choixModeJeu;
            Console.WriteLine("Voulez-vous changer le premier joueur [1], le second joueur [2], les deux [3] ?");
            while (!int.TryParse(Console.ReadLine(),out choixModeJeu)){
                Console.WriteLine("Valeur erronée, veuillez entrer 1, 2 ou 3.");
            }
            if (choixModeJeu== 1) {
                Console.WriteLine("Aurevoir {0} ! Votre score était de {1}.",j1.nom,j1.nbVictoire);
                j1.nbVictoire=0;
                j1.aInitialiser=true;
                initialisationJoueur (ref j1,ref j2);
            }
            if (choixModeJeu== 2) {
                Console.WriteLine("Aurevoir {0} ! Votre score était de {1}.",j2.nom,j2.nbVictoire);
                j2.aInitialiser=true;
                initialisationJoueur (ref j1,ref j2);
            }
            if (choixModeJeu== 3) {
                messageFin(ref j1,ref j2);
                j1.aInitialiser=true;
                j2.aInitialiser=true;
                initialisationJoueur (ref j1,ref j2);
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


        static void Main(string[] args)
        {
            int taillePendu=0;
            int niveau=0;
            bool continuerAJouer=true;
            bool changement;
            bool perdu = false;
            string coup;
            char [] mot, lettresDecouvertes;
            Joueur j1 = new Joueur();
            Joueur j2 = new Joueur();
            List<string> lettresDejaJouees = new List<string>();

            chargeFichier(ADRESSE_DICO, Fichier.dictionnaire);
            chargeFichier(ADRESSE_REGLES, Fichier.regles);

            
            j1.aInitialiser=true;
            j2.aInitialiser=true;
            initialisationJoueur (ref j1,ref j2);
            initialisationDictionnaires ();

            ChoixNiveau(ref niveau);



            while (continuerAJouer){
                Console.Clear();
                // choix du mot à faire deviner
                if (j1.role==CHOIX_MOT) choixMot(ref j1,out mot, out lettresDecouvertes);
                else                    choixMot(ref j2,out mot, out lettresDecouvertes);

                // l'autre joueur tente de deviner avec max 5 erreurs
                while (!(perdu || deepEqualsTabChar(mot,lettresDecouvertes))){
                    dessinePendu(taillePendu);
                    afficheTab(lettresDecouvertes);
                    Console.Write("\nLettres déjà jouées : ");
                    afficheListe(lettresDejaJouees,27,0);

                    if (j1.role==DEVINE)  coup=JoueCoup(ref j1,lettresDejaJouees,lettresDecouvertes, niveau);
                    else                  coup=JoueCoup(ref j2,lettresDejaJouees,lettresDecouvertes, niveau);

                    if (coup.Equals(ABANDON)) perdu = true ;
                    if (coup.Equals(AIDE)) {
                        coup = coupIntelligent(lettresDejaJouees, lettresDecouvertes) ;
                        Console.WriteLine("L'ordinateur choisit pour vous la réponse \"{0}\"",coup);
                        if (coup.Length==1) lettresDejaJouees.Add(coup);
                        Thread.Sleep(TEMPS_ATTENTE);
                    }
                    
                    else if (coup.Length==1){
                        if (!isLettreDansMot(char.Parse(coup), mot, lettresDecouvertes)){
                            taillePendu++;
                        }
                    }
                    else {
                        if (deepEqualsTabChar(coup.ToCharArray(),mot)){
                            lettresDecouvertes=mot;
                        }
                        else {
                            perdu=true;
                        }
                    }
                    if (taillePendu==MAX_PENDU) perdu=true;                         
                }

                if (perdu){
                    dessinePendu(taillePendu);
                    Console.WriteLine ("{0}, vous avez perdu ! Le mot a deviner était :",(j1.role==DEVINE)?j1.nom:j2.nom);
                    afficheTab(mot);
                }
                else {
                    Console.WriteLine ("{0}, vous avez gagné ! Le mot a deviner était :",(j1.role==DEVINE)?j1.nom:j2.nom);
                    afficheTab(mot);
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
                    if (changement) changementModeJeu (ref j1, ref j2);


                    Console.WriteLine("Voulez-vous changer de niveau [true/false] ?");
                    while (!bool.TryParse(Console.ReadLine(),out changement)){
                        Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                    }
                    if (changement) ChoixNiveau(ref niveau);
                }
                
                j1.role=!j1.role;
                j2.role=!j2.role;

                //réinitialisation des variable
                lettresDejaJouees.Clear();
                dictionnaireCourant.Remove(new String(mot));
                perdu = false ;
                taillePendu=0;
            }

            messageFin(ref j1,ref j2);
        }
    }
}
