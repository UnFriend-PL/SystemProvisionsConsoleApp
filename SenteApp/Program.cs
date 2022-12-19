using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SenteApp
{
    internal class Program
    {
        static string structurePath =  @$"struktura.xml";
        static string transfersPath = @$"przelewy.xml";
        static XmlDocument structureXML = new XmlDocument();
        static XmlDocument transfersXML = new XmlDocument();
        static void Main(string[] args)
        {
            Dictionary<int, Participant> participantDictionary = new Dictionary<int, Participant>(); // Mapa wg Id do Uczestnika
            structureXML.Load(structurePath);
            transfersXML.Load(transfersPath);
            XmlNodeList participantNodes = structureXML.SelectNodes("//uczestnik"); // Lista uczestnikow z xml
            XmlNodeList transfers = transfersXML.SelectNodes("//przelew"); // Lista przelewow z xml

            foreach (XmlNode participantNode in participantNodes) // iterowanie kazdego elementu z listy uczestnikow (participantNodes)
            {
                XmlNode parentNodde = participantNode.ParentNode; // pobranie tagu uczestnika wyzej(rodzica)
                Participant participant = new Participant();
                int participantID = int.Parse(participantNode.Attributes["id"].Value); // pobranie ID obecnego uczestnika
                participant.Id = participantID;
                participantDictionary.Add(participantID, participant); // Dodanie Klucza i wartosci ddo Mapy
                if (parentNodde != null && parentNodde.Attributes.Count > 0)
                {
                    int parentId = Int32.Parse(parentNodde.Attributes["id"].Value); // pobranie Id uczestnika wyzej(rodzica)
                    participant.Parent = participantDictionary[parentId]; // przypisanie obecnemu uczestnikowi rodzica wg klucza z mapy
                }
            }

            foreach (XmlNode transfer in transfers)
            {
                double amount = double.Parse(transfer.Attributes["kwota"].Value); // pobranie wartosci wplaty
                int userID = int.Parse(transfer.Attributes["od"].Value); // pobranie Id uczestnika, od ktorego zostala wykonana wplata
                Participant participant = participantDictionary[userID]; 
                List<Participant> participantParents = new List<Participant>(); // Stworzenie Listy, ktora potem bedzie drzewkiem wplat
                populateParentChain(participant, participantParents); // wypelniennie listy odpowiednimi uczestnikami
                participantParents.Reverse(); // oddwracamy kolejnosc listy, zeby byla od zalozyciela
                distributeTransfer(participantParents, amount); // liczenie prowizji i przypisanie do uczestnika
            }
            foreach (var participantPair in participantDictionary.OrderBy(x => x.Value.Id).ToList()) // ODDPOWIEDZ
            {
                Console.WriteLine($"{participantPair.Value.Id}  {participantPair.Value.getLvl()}  {participantPair.Value.countChildWithoutChildren()}  {participantPair.Value.Provision}");
            }
            Console.WriteLine("\n Czytelne dla ludzi:");
            foreach (var participantPair in participantDictionary.OrderBy(x => x.Value.Id).ToList()) // ODDPOWIEDZ szczegolowa
            {
                Console.WriteLine($"Uczestnik ID: {participantPair.Value.Id}\n lvl: {participantPair.Value.getLvl()} \n pracownicy bez podwładnych: {participantPair.Value.countChildWithoutChildren()} \n Prowizja: {participantPair.Value.Provision}");
            }

        }
        public static void populateParentChain(Participant participant, List<Participant> parents)
        {
            if (participant.Parent!= null) // konczy sie gdyd dojdzie do zalozyciela - nie ma referencji wyzej
            {
                parents.Add(participant.Parent); // ddodanie ddo drzewka wyzej postawionego uczestnika
                populateParentChain(participant.Parent, parents); // rekurencja dla rodzica obecnego uczestnika 
            }
        }
        public static void distributeTransfer(List<Participant> participantParents, double amount)
        {
            if (participantParents.Count > 1) // Sprawdzenie, czy w drzewku jest wiecej niz jedna osoba, aby oddpowiednio obliczyc prowizje
            {
                double provision = Math.Floor(amount * 0.5); // liczenie prowizji
                participantParents[0].Provision += provision; // ddodanie prowizji
                participantParents.RemoveAt(0); // usuniecie osoby z drzewka
                distributeTransfer(participantParents, amount - provision); // rekurencja z nowa lista juz bez osoby ddla ktorej obliczylismy prowizje
            }
            else if (participantParents.Count > 0) // jesli jest to ostatnia osoba w drzewku to przypisujemy cala kwote
            {
                participantParents[0].Provision += amount;
            }
        }
    }
}
