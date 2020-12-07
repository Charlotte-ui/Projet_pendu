
using System;
using System.Collections.Generic;
using System.IO ;
using System.Linq;
using System.Collections;

namespace Projet_pendu
{
    class Program
    {
        public const bool VERBOSE = false;
        public const bool CHOIX_MOT = true;
        public const bool DEVINE = false;
        public const int MAX_PENDU = 5 ;
        public const string ADRESSE_DICO = "dicoFR.txt" ;
        public const string ADRESSE_REGLES = "regles.txt" ;
        public static List<string> dictionnaire;
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
        }

        // reste le test de si contient des caractères non autorisés
         public static string JoueCoup (Joueur j,List<string> lettresDejaJouees, char[] lettresDecouvertes) {            
             if (VERBOSE) Console.WriteLine("entrée dans JoueCoup");
            string reponse;
           // int
            if (j.robot){
                if (VERBOSE) Console.WriteLine("le robot joue un coup");
                Console.Write(j.nom);
                //return CoupAleatoire (lettresDejaJouees);
                return CoupIntelligent(lettresDejaJouees,lettresDecouvertes);
            }
            else {
                if (VERBOSE) Console.WriteLine("l'humain joue un coup");
                Console.WriteLine("{0}, quelle lettre ou mot proposez vous ? (entrer [1] pour abandonner, [2] pour afficher les règles, [3] pour recevoir une aide intelligente de l'ordinateur)", j.nom);
                reponse = Console.ReadLine().ToUpper();
                if (reponse.Equals("2")) afficheRegles();
                while ((lettresDejaJouees.Contains(reponse) || !isChaineLegal(reponse) || reponse.Equals("2")) && !reponse.Equals("1") && !reponse.Equals("3")){
                    if (!isChaineLegal(reponse) && !reponse.Equals("2"))  Console.WriteLine("Vous avez saisi un caractères non autorisé, veuillez recommencer.");
                    else if (lettresDejaJouees.Contains(reponse)) Console.WriteLine("Cette lettre a déjà été jouez, choisissez en une autre.");
                    else  Console.WriteLine("{0}, quelle lettre ou mot proposez vous ? (entrer [1] pour abandonner, [2] pour afficher les règles, [3] pour recevoir une aide intelligente de l'ordinateur)", j.nom);
                    reponse = Console.ReadLine().ToUpper();               
                }
                if (reponse.Length==1 && !reponse.Equals("3")) lettresDejaJouees.Add(reponse) ;
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
      
        public static string CoupIntelligent (List<string> lettresDejaJouees, char[] lettresDecouvertes){    
            List<char> lettresAbsentes = new List<char>();
            List<string> motCompatibles = new List<string>();

            foreach (string s in lettresDejaJouees)
            {
                if(!lettresDecouvertes.Contains(Char.Parse(s))) lettresAbsentes.Add(Char.Parse(s));
            }

            foreach(string mot in dictionnaire){
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


        public static bool estCompatible (string mot, char[] lettresDecouvertes,List<char> lettresAbsentes){
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


        public static void choixMot (Joueur j, out char[] mot, out char[] lettresDecouvertes){
	        int indexDico;
            bool motAccepte =false;
            string reponse="1";

            //mot = new char[] {'C','H','A','T'};     
            //lettresDecouvertes = new char [] { '_', '_', '_','_','_'};   
 
			Random rndIndex = new Random();
			if(j.robot == true){
				indexDico = rndIndex.Next(0, dictionnaire.Count);
				mot = dictionnaire[indexDico].ToCharArray();
			}
			else{
                while (!motAccepte) {
                    while (reponse.Equals("1")){
                        Console.WriteLine("Choisissez un mot parmi la liste. Taper [1] pour afficher la liste");
                        reponse = Console.ReadLine();
                        if (reponse.Equals("1")) afficheListe(dictionnaire,100);
                    }
                    
                    motAccepte = dictionnaire.Contains(reponse) && isChaineLegal(reponse);
                    if (!motAccepte) {
                        Console.WriteLine("Le mot est introuvable sur le dictionnaire. Veuillez réessayer.");
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

        public static void afficheListe (List<string> l, int limite){
            if (l.Count<limite) limite=l.Count;
            for (int i=0;i<limite;i++){
                Console.Write(l[i]);
                Console.Write(" ");
            }
            Console.WriteLine();
        }

        public static void afficheRegles (){
            foreach (string ligne in regles){
                centrerLeTexte(ligne);
            }

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





        static void Main(string[] args)
        {
            int choixModeJeu;
            int taillePendu=0;
            bool continuerAJouer=true;
            bool perdu = false;
            string coup;
            char [] mot, lettresDecouvertes;
            Joueur j1 = new Joueur();
            Joueur j2 = new Joueur();
            List<string> lettresDejaJouees = new List<string>();

            chargeFichier(ADRESSE_DICO, Fichier.dictionnaire);
            chargeFichier(ADRESSE_REGLES, Fichier.regles);

            Console.WriteLine("Vous désirez jouer avec : deux ordinateurs [1], deux humains [2], un ordinateur contre un humain [3] ?");
            while (!int.TryParse(Console.ReadLine(),out choixModeJeu) ||  choixModeJeu<1 ||  choixModeJeu>3 ){
                Console.WriteLine("Valeur erronée, veuillez entrer un entier 1, 2 ou 3 en fonction du mode de jeu désiré.");
            } 

            switch (choixModeJeu) {
                case 1:
                j1.nom="HAL";
                j2.nom="Skynet";
                j1.robot=true;
                j2.robot=true;
                break;
                case 2:
                demandeNom(ref j1.nom,"du premier joueur");
                demandeNom(ref j2.nom,"du second joueur");
                j1.robot=false;
                j2.robot=false;
                break;
                case 3:
                demandeNom(ref j1.nom,"du premier joueur");
                j2.nom="Skynet";
                j1.robot=false;
                j2.robot=true;
                break;

            }
            j1.role = (new Random().Next(0, 2) ==0)? CHOIX_MOT: DEVINE;
            j2.role = !j1.role;

            while (continuerAJouer){
                // choix du mot à faire deviner
                if (j1.role==CHOIX_MOT) choixMot(j1,out mot, out lettresDecouvertes);
                else                    choixMot(j2,out mot, out lettresDecouvertes);

                // l'autre joueur tente de deviner avec max 5 erreurs
                while (!(perdu || deepEqualsTabChar(mot,lettresDecouvertes))){
                    dessinePendu(taillePendu);
                    afficheTab(lettresDecouvertes);
                    Console.Write("Lettes déjà jouées : ");
                    afficheListe(lettresDejaJouees,27);

                    
                    if (j1.role==DEVINE)  coup=JoueCoup(j1,lettresDejaJouees,lettresDecouvertes);
                    else                  coup=JoueCoup(j2,lettresDejaJouees,lettresDecouvertes);

                    if (coup.Equals("1")) perdu = true ;
                    if (coup.Equals("3")) coup = CoupIntelligent(lettresDejaJouees, lettresDecouvertes) ;
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
                    Console.WriteLine ("{0}, vous avez perdu ! le mot a deviner était :",(j1.role==DEVINE)?j1.nom:j2.nom);
                    afficheTab(mot);
                }
                else {
                    Console.WriteLine ("{0}, vous avez gagné !",(j1.role==DEVINE)?j1.nom:j2.nom);
                    if (j1.role==DEVINE) j1.nbVictoire++;
                    else j2.nbVictoire++;
                }


                Console.WriteLine(" Voulez-vous faire une nouvelle partie [true/false] ?");
                while (!bool.TryParse(Console.ReadLine(),out continuerAJouer)){
                    Console.WriteLine("Valeur erronée, veuillez entrer \"true\" ou \"false\".");
                }

                j1.role=!j1.role;
                j2.role=!j2.role;

                //réinitialisation des variable
                lettresDejaJouees.Clear();
                dictionnaire.Remove(new String(mot));
                perdu = false ;
                taillePendu=0;
                

            }

            Console.WriteLine("Fin de partie \n score {0} : {1} \n score {2} : {3} ",j1.nom,j1.nbVictoire,j2.nom,j2.nbVictoire);
        }
    }
}
