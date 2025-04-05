using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jäsenrekisteriohjelma_MongoDb
{
    public class Jasen
    {
        public ObjectId Id { get; set; }
        public string Etunimi { get; set; }
        public string Sukunimi {  get; set; }
        public string Osoite { get; set; }
        public int Postinumero { get; set; }
        public int Puhelin {  get; set; }
        public string Sahkoposti { get; set; }
        public DateTime? JasenydenAlkuPvm {  get; set; }
    }
}
