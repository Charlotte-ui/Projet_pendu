
using System;
using System.Collections.Generic;
using System.IO ;
using System.Linq;
using System.Collections;

namespace Projet_pendu
{
    class Program
    {
        public const bool VERBOSE = true;
        public const bool CHOIX_MOT = true;
        public const bool DEVINE = false;
        public const int MAX_PENDU = 5 ;
        public const string ADRESSE_DICO = "dicoFR.txt" ;
        public static List<string> dictionnaire;




        public struct Joueur {
            public string nom;
            public bool robot;
            public int nbVictoire;
            public bool role ; //true choisit mot, false devine
        }

        // reste le test de si contient des caractères non autorisés
         public static string JoueCoup (Joueur j,List<string> lettresDejaJouees, char[] lettresDecouvertes) {            if (VERBOSE) Console.WriteLine("entrée dans JoueCoup");
            string reponse;
            if (j.robot){
                if (VERBOSE) Console.WriteLine("le robot joue un coup");
                Console.Write(j.nom);
                //return CoupAleatoire (lettresDejaJouees);
                return CoupIntelligent(lettresDejaJouees,lettresDecouvertes);
            }
            else {
                if (VERBOSE) Console.WriteLine("l'humain joue un coup");
                Console.WriteLine("{0}, quelle lettre ou mot proposez vous ? ", j.nom);
                reponse = Console.ReadLine().ToUpper();
                while (lettresDejaJouees.Contains(reponse) || !isChaineLegal(reponse)){
                    if (!isChaineLegal(reponse))  Console.WriteLine("Vous avez saisi un caractères non autorisé, veuillez recommencer.");
                    else Console.WriteLine("Cette lettre a déjà été jouez, choisissez en une autre.");
                    reponse = Console.ReadLine().ToUpper();               
                }
                if (reponse.Length==1) lettresDejaJouees.Add(reponse) ;
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
            return true;
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

        public static void chargeDictionnaire (string adresse) {
            dictionnaire = new List<string>();
            try 
            { 
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("iso-8859-1");
                StreamReader monStreamReader = new StreamReader(adresse,encoding); 
                string mot = monStreamReader.ReadLine(); 
                while (mot != null) { 
                    dictionnaire.Add(mot);
                    mot = monStreamReader.ReadLine();
                } 
                monStreamReader.Close(); 
            } 
            catch (Exception ex) 
            { 
                Console.Write("Une erreur est survenue au cours de la lecture :"); 
                Console.WriteLine(ex.Message); 
            } 
        }

        public static void choixMot (Joueur j, out char[] mot, out char[] lettresDecouvertes){
	        
                //mot = new char[] {'C','H','A','T'};            

				Random rndIndex = new Random();
				if(j.robot == true){
					indexDico = rndIndex(0, dictionnaire.Size());
					mot = dictionnaire[indexDico];
				}
				else{
                    while (!motAccepte) {
                    bool motAccepte = false;
                        while (reponse == '1')
                        {
                            Console.WriteLine("Choisissez un mot parmi la liste. Taper [1] pour afficher la liste");
                            string reponse = Console.ReadLine();
                            if (reponse == '1') afficheListe();
                            else mot = reponse;
                        }
                    
                    if (dictionnaire.Contains(mot)) {
                        if (isChaineLegal(mot) == true) {
                            motAccepte = true; 
                        }
                    }
                    else {
                        Console.WriteLine("Le mot est introuvable sur le dictionnaire. Veuillez réessayer.");
                    }
				}
				lettresDecouvertes = new char [] { '_', '_', '_'};
            /*char[] alphabet={'a','b','c','d','e','f','g','h','i','j','k','l','m',
			'n','o','p','q','r','s','t','u','v','w','x','y','z',' '};
            for(int i = 0; i < mot.Length-1; i++){
                int k = 0;
                bool onPasseAuCaracSuiv = false;
                while(k < alphabet.Length-1 || onPasseAuCaracSuiv == true){
                    if(mot[i] == alphabet[k]){ 
                        onPasseAuCaracSuiv = true;
                    }
                    k++;
                }
            }*/
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


        public static void dessinePendu (int taille){
            CentrerLeTexte(" _______");
            CentrerLeTexte(" |/   | ");
            switch (taille) {
            case 0 :
                CentrerLeTexte(" |      ");
                CentrerLeTexte(" |      ");
                CentrerLeTexte(" |      ");
                CentrerLeTexte(" |      ");
                break;
            case 1 :
            CentrerLeTexte(" |    O ");
            CentrerLeTexte(" |      ");
            CentrerLeTexte(" |      ");
            break;
            case 2 :
            CentrerLeTexte(" |    O ");
            CentrerLeTexte(" |   -| ");
            CentrerLeTexte(" |      ");
            break;
            case 3 :
            CentrerLeTexte(" |    O ");
            CentrerLeTexte(" |   -|-");
            CentrerLeTexte(" |      ");
            break;
            case 4 :
            CentrerLeTexte(" |    O ");
            CentrerLeTexte(" |   -|-");
            CentrerLeTexte(" |    / ");
            break;
            case 5 :
            CentrerLeTexte(" |    O ");
            CentrerLeTexte(" |   -|-");
            CentrerLeTexte(" |    /\\");
            break;
            }



            CentrerLeTexte("-----------");




            
        }

        private static void CentrerLeTexte(string texte){
            int nbEspaces = (Console.WindowWidth - texte.Length) / 2;
            Console.SetCursorPosition(nbEspaces, Console.CursorTop);
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

            chargeDictionnaire(ADRESSE_DICO);

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
                if (j1.role==DEVINE)  coup=JoueCoup(j1,lettresDejaJouees,lettresDecouvertes);
                    else              coup=JoueCoup(j2,lettresDejaJouees,lettresDecouvertes);

                // l'autre joueur tente de deviner avec max 5 erreurs
                while (!(perdu || deepEqualsTabChar(mot,lettresDecouvertes))){
                    dessinePendu(taillePendu);
                    afficheTab(lettresDecouvertes);
                    Console.Write("Lettes déjà jouées : ");
                    afficheListe(lettresDejaJouees,27);

                    
                    if (j1.role==DEVINE)  coup=JoueCoup(j1,lettresDejaJouees);
                    else                  coup=JoueCoup(j2,lettresDejaJouees);

                    if (coup.Length==1){
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
