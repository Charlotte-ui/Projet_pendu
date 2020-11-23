using System;
using System.Collections.Generic;

namespace Projet_pendu
{
    class Program
    {
        public const bool CHOIX_MOT = true;
        public const bool DEVINE = false;
        public const int MAX_PENDU = 5 ;

        public static List<string> lettresDejaJouees;
        public static List<string> dictionnaire;




        public struct Joueur {
            string nom;
            bool robot;
            int nbVictoire;
            bool role ; //true choisit mot, false devine
        }

        public string JoueCoup (Joueur j) {
            return "";
        }

        public bool isLettreDansMot (char lettre, char[] mot, char[] lettresDecouvertes){
            return false;
        }

        public bool deepEqualsTabChar (char[] tab1, char[] tab2) {
            return false;
        }

        public void chargeDictionnaire (string adresse) {

        }

        public void choixMot (bool robot, char[] mot, char[] lettresDecouvertes){
        }

        public void afficheTab (char[] tab){

        }


        public void dessinePendu (int taille){
            
        }





        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            List<int> chiffres = new List<int>(); // création de la liste
        }
    }
}
