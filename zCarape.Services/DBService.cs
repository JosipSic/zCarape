using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using zCarape.Core;
using zCarape.Core.Business;
using zCarape.Core.Models;
using zCarape.Services.Interfaces;

namespace zCarape.Services
{
    public class DBService : IDbService
    {
        /// <summary>
        /// Komanda nad postojecim podacima (UPDATE ili DELETE)
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ocekivaniBrojZapisa">Ako je parametar prosledjen i ako je tangirani broj zapisa veci od 0 i razlicit od ocekivanog procedura javlja upozorenje</param>
        /// <returns></returns>
        private int IzvrsiCmdNonQuery(SQLiteCommand cmd, int ocekivaniBrojZapisa=0)
        {
            int brojZapisa = 0;
            try
            {
                brojZapisa = cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
            }

            // Ako je akcija izvrsena nad razlicitim brojem zapisa od ocekivanog javljam poruku
            if (ocekivaniBrojZapisa > 0 && brojZapisa > 0 && ocekivaniBrojZapisa != brojZapisa )
            {
                MessageBox.Show($"Broj tangiranih zapisa u bazi {brojZapisa} se razlikuje od broja očekivanih zapisa {ocekivaniBrojZapisa}."
                    , "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return brojZapisa;
        }

        #region Odrzavanje
        private bool checkIfColumnExists(string tableName, string columnName)
        {
            using (var conn = new SQLiteConnection(GlobalniKod.ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                { 
                    cmd.CommandText = string.Format("PRAGMA table_info({0})", tableName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        int nameIndex = reader.GetOrdinal("Name");
                        while (reader.Read())
                        {
                            if (reader.GetString(nameIndex).Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                conn.Close();
                                return true;
                            }
                        }
                    }
                }
                conn.Close();
            }
            return false;
        }

        private void postaviBrojVerzijeUBazi(int verzija)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@naziv", "DBVER");
            cmd.Parameters.AddWithValue("@vrednost", verzija);

            cmd.CommandText = "UPDATE param SET vrednost=@vrednost WHERE naziv=@naziv";

            cmd.ExecuteNonQuery();
        }

        private bool dodajPoljeUTabelu(string tabela, string polje, string cmdText)
        {
            bool uspeh = true;
            if (!checkIfColumnExists(tabela, polje))
            {

                try
                {
                    using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
                    con.Open();

                    using var cmd = new SQLiteCommand(con);
                    cmd.CommandText = cmdText;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    uspeh = false;
                }
            }
            return uspeh;
        }

        public bool TestConnection(out string poruka)
        {
            poruka = string.Empty;
            // Prvo proveravam da li fizicki postoji na lokaciji
            if (!File.Exists(GlobalniKod.BazaPath))
            {
                poruka = string.Format("Baza \"{0}\" ne postoji na trazenoj lokaciji.", GlobalniKod.BazaPath);
                return false;
            }

            //Proveravam da li koristim zahtevanu verziju
            //1 - Da li postoji tabela Param i red sa Nazivom Verzija i ako ne postoji kreiram je
            bool tabelaParamPostoji = false;
            try
            {
                using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
                con.Open();
                using var cmd = new SQLiteCommand(con);

                // Da li postoji tabela Param
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='param'";
                tabelaParamPostoji = cmd.ExecuteNonQuery() == 1;
                if (!tabelaParamPostoji)
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS 'Param' ('Naziv' TEXT NOT NULL,'Vrednost' INTEGER NOT NULL, PRIMARY KEY('Naziv'))";
                    cmd.ExecuteNonQuery();
                }

                // Da li postoji red DBVER
                cmd.Parameters.AddWithValue("@naziv", "DBVER");
                cmd.CommandText = "SELECT COUNT(*) FROM Param WHERE Naziv=@naziv";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    cmd.CommandText = "INSERT INTO Param(naziv,vrednost) VALUES (@naziv,0)";
                    cmd.ExecuteNonQuery();
                }

            }
            catch (System.Exception ex)
            {
                poruka = ex.Message;
                return false;
            }

            return true;
        }

        public bool NadogradiBazu(out string poruka)
        {
            poruka = string.Empty;
            int _verzijaBaze = 0;
            // Koju verziju koristim
            try
            {
                using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
                con.Open();

                using var cmd = new SQLiteCommand(con);
                cmd.CommandText = "SELECT vrednost FROM Param WHERE naziv=@naziv";
                cmd.Parameters.AddWithValue("@naziv", "DBVER");

                _verzijaBaze = Convert.ToInt32(cmd.ExecuteScalar());

            }
            catch (Exception ex)
            {
                poruka = ex.Message;
                return false;
            }

            // Nadogradnja baze
            bool sveJeOK = true;
            int potrebnaVezija;

            // Verzija 2
            potrebnaVezija = 2;
            if (_verzijaBaze < potrebnaVezija)
            {
                string tabela, polje, cmdText;

                ;
                // dodajem polje Putanja u tabelu DezeniArtikla
                tabela = "DezeniArtikla";
                polje = "Putanja";
                cmdText= string.Format($"ALTER TABLE {tabela} ADD COLUMN {polje} TEXT");
                if (!dodajPoljeUTabelu(tabela, polje, cmdText)) { sveJeOK = false; }

                // dodajem polje RadniNalogID u tabelu Masine
                tabela = "Masine";
                polje = "RadniNalogID";
                cmdText = string.Format($"ALTER TABLE {tabela} ADD COLUMN {polje} INTEGER NOT NULL DEFAULT 0");
                if (!dodajPoljeUTabelu(tabela, polje, cmdText)) { sveJeOK = false; }


                //  kreiram tabelu AngazovaneMasine
                try
                {
                    using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
                    con.Open();
                    using var cmd = new SQLiteCommand(con);
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS 'AngazovaneMasine' (" +
                                        "'ID' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                                        "'RadniNalogID' INTEGER NOT NULL," +
                                        "'MasinaID' INTEGER NOT NULL," +
                                        "'Status' INTEGER NOT NULL DEFAULT 1," +
                                        "'Redosled' INTEGER NOT NULL DEFAULT 1000," +
                                        "FOREIGN KEY('RadniNalogID') REFERENCES 'RadniNalozi'('ID')," +
                                        "FOREIGN KEY('MasinaID') REFERENCES 'Masine'('ID'))";
                    cmd.ExecuteNonQuery();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    sveJeOK = false;
                }


                if (sveJeOK) postaviBrojVerzijeUBazi(potrebnaVezija);
            }

            // Verzija 3
            potrebnaVezija = 3;
            if (_verzijaBaze < potrebnaVezija)
            {
                //  kreiram tabelu Lica
                try
                {
                    using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
                    con.Open();
                    using var cmd = new SQLiteCommand(con);
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS 'Lica' (" +
                                        "'ID' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                                        "'Ime' TEXT NOT NULL," +
                                        "'Prezime' TEXT," +
                                        "'RadnoMesto' TEXT," +
                                        "'Aktivan' INTEGER NOT NULL DEFAULT 1," +
                                        "'VremeUnosa' TEXT NOT NULL DEFAULT current_timestamp)";
                    cmd.ExecuteNonQuery();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    sveJeOK = false;
                }


                //  kreiram tabelu Predajnice
                try
                {
                    using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
                    con.Open();
                    using var cmd = new SQLiteCommand(con);
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS 'Predajnice' (" +
                                        "'ID' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                                        "'RadniNalogID' INTEGER NOT NULL," +
                                        "'MasinaID' INTEGER NOT NULL," +
                                        "'Datum' Text NOT NULL," +
                                        "'Smena' INTEGER NOT NULL," +
                                        "'LiceID' INTEGER NOT NULL," +
                                        "'Kolicina' INTEGER NOT NULL," +
                                        "'DrugaKl' INTEGER NOT NULL," +
                                        "'VremeUnosa' TEXT NOT NULL DEFAULT current_timestamp,"+
                                        "FOREIGN KEY('RadniNalogID') REFERENCES 'RadniNalozi'('ID')," +
                                        "FOREIGN KEY('LiceID') REFERENCES 'Lica'('ID')," +
                                        "FOREIGN KEY('MasinaID') REFERENCES 'Masine'('ID'))";
                    cmd.ExecuteNonQuery();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    sveJeOK = false;
                }


                if (sveJeOK) postaviBrojVerzijeUBazi(potrebnaVezija);
            }

            return sveJeOK;
        }

        #endregion //PrivatneMetode

        #region Masine
        public IEnumerable<MasinaURadu> GetAllMasineURadu(DateTime _datum)
        {
            // Povlacim iz baze sve predajnice na trazeni datum
            IEnumerable<Predajnica> predajniceNaDan = GetAllPredajniceOnDate(_datum);

            List<MasinaURadu> masineURadu = new List<MasinaURadu>();

            #region Za testiranje
            /*
            string slika1 =  Path.Combine(GlobalniKod.SlikeDir, "Bebino_Hulahop.jpg");
            string slika2 = Path.Combine(GlobalniKod.SlikeDir, "Muska_Termo_Carapa.jpg");
            string slika3 = Path.Combine(GlobalniKod.SlikeDir, "Muška sportska čarapa.jpg");
            string slika4 = Path.Combine(GlobalniKod.SlikeDir, "Zenska_Pamucna_Sokna.jpg");
            string slika5 = Path.Combine(GlobalniKod.SlikeDir, "Klasicna_Carapa_12.jpg");
            masineURadu.Add(new MasinaURadu()
            {
                MasinaID = 1,
                MasinaNaziv = "156 A",
                NaloziURadu = new List<NalogURadu>()
                {
                    new NalogURadu() { RadniNalogID=101, ArtikalID=1, ArtikalNaziv="Trend sokna muska", ArtikalVelicina="44/45", ArtikalDezen="Crna", Cilj=700, PutanjaFajla=@"2020\168\TrendSocks\44-45\"
                    ,Slika = slika1 },
                }
            });

            masineURadu.Add(new MasinaURadu()
            {
                MasinaID = 2,
                MasinaNaziv = "168 A",
                NaloziURadu = new List<NalogURadu>()
                {
                    new NalogURadu() { RadniNalogID=101, ArtikalID=1, ArtikalNaziv="Mediko", ArtikalVelicina="39/42", ArtikalDezen="Antracid", Cilj=1000, PutanjaFajla=@"2020\156A\Mediko\39-42\mana", Slika=slika2},
                    new NalogURadu() { RadniNalogID=102, ArtikalID=9, ArtikalNaziv="Sportska", ArtikalVelicina="42/43", ArtikalDezen="Plavo bela", Cilj=6500, PutanjaFajla=@"2020\156A\Sportska\42-43\plavobela", Slika=slika3},
                    new NalogURadu() { RadniNalogID=103, ArtikalID=3, ArtikalNaziv="Bebino", ArtikalVelicina="2,4", ArtikalDezen="Plavi automobili", Cilj=350, PutanjaFajla=@"2020\156A\Decije\automobilcici\2-4\maliplavi", Slika=slika4},

                }
            });


            masineURadu.Add(new MasinaURadu()
            {
                MasinaID = 3,
                MasinaNaziv = "XP6 156B",
                NaloziURadu = new List<NalogURadu>()
                {
                    new NalogURadu() { RadniNalogID=104, ArtikalID=26, ArtikalNaziv="Hulahop lovacki", ArtikalVelicina="univerzalna", ArtikalDezen="Rebrast", Cilj=100, PutanjaFajla=@"2020\156\Hulahop\Lovacki\Rebrast", Slika=slika5},
                }
            });
            */

            #endregion

            // Masine koje imaju aktivne radne naloge
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT Masine.ID as MasinaID, Masine.Naziv as MasinaNaziv," +
                "RadniNalozi.ID as RadniNalogID, RadniNalozi.ArtikalID, RadniNalozi.Cilj, RadniNalozi.HItno," +
                "RadniNalozi.Status as StatusNaloga, RadniNalozi.Podsetnik, Artikli.Sifra as ArtikalSifra," +
                "Artikli.Naziv as ArtikalNaziv, Velicine.Oznaka as ArtikalVelicina, DezeniArtikla.Naziv as ArtikalDezen," +
                "DezeniArtikla.Slika1, DezeniArtikla.Slika2, DezeniArtikla.Slika3, DezeniArtikla.Putanja as PutanjaFajla," +
                "AngazovaneMasine.ID, AngazovaneMasine.Redosled, AngazovaneMasine.Status as StatusMasine " +
                "FROM Masine " +
                "INNER JOIN AngazovaneMasine ON  AngazovaneMasine.MasinaID = Masine.ID " +
                "INNER JOIN RadniNalozi ON RadniNalozi.ID = AngazovaneMasine.RadniNalogID " +
                "INNER JOIN Artikli ON RadniNalozi.ArtikalID = Artikli.ID " +
                "INNER JOIN Velicine ON RadniNalozi.VelicinaID = Velicine.ID " +
                "INNER JOIN DezeniArtikla ON RadniNalozi.DezenArtiklaID = DezeniArtikla.ID " +
                "WHERE RadniNalozi.Status != 9 " +
                "ORDER BY Masine.Naziv, RadniNalozi.Hitno DESC, RadniNalozi.Status, AngazovaneMasine.Redosled, RadniNalozi.ID";

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                long id;
                long masinaID;
                string masinaNaziv;
                long radniNalogID;
                long artikalID;
                long cilj;
                bool hitno;
                byte statusNaloga;
                string artikalSifra;
                string artikalNaziv;
                string artikalVelicina;
                string artikalDezen;
                string slika1;
                string slika2;
                string slika3;
                string putanjaFajla;
                int redosled;
                byte statusMasine;
                string podsetnik;
                bool canGoLeft;

                bool lastHitno=false;
                byte lastStatusNaloga=1;

                while (dr.Read())
                {
                    id = (long)dr["id"];
                    masinaID = (long)dr["masinaID"];
                    masinaNaziv = (dr["masinaNaziv"] == DBNull.Value) ? string.Empty : (string)dr["masinaNaziv"];
                    radniNalogID = (long)dr["radniNalogID"];
                    artikalID = (long)dr["artikalID"];
                    cilj = (long)dr["cilj"];
                    hitno = (long)dr["hitno"]==1; 
                    statusNaloga = (byte)(long)dr["statusNaloga"];
                    artikalSifra = (dr["artikalSifra"] == DBNull.Value) ? string.Empty : (string)dr["artikalSifra"];
                    artikalNaziv = (dr["artikalNaziv"] == DBNull.Value) ? string.Empty : (string)dr["artikalNaziv"];
                    artikalVelicina = (dr["artikalVelicina"] == DBNull.Value) ? string.Empty : (string)dr["artikalVelicina"];
                    artikalDezen = (dr["artikalDezen"] == DBNull.Value) ? string.Empty : (string)dr["artikalDezen"];
                    slika1 = (dr["slika1"] == DBNull.Value) ? string.Empty : (string)dr["slika1"];
                    slika2 = (dr["slika2"] == DBNull.Value) ? string.Empty : (string)dr["slika2"];
                    slika3 = (dr["slika3"] == DBNull.Value) ? string.Empty : (string)dr["slika3"];
                    putanjaFajla = (dr["putanjaFajla"] == DBNull.Value) ? string.Empty : (string)dr["putanjaFajla"];
                    redosled = (int)(long)dr["redosled"];
                    statusMasine = (byte)(long)dr["statusMasine"];
                    podsetnik = (dr["podsetnik"] == DBNull.Value) ? string.Empty : (string)dr["podsetnik"];

                    // Masina
                    MasinaURadu masinaURadu = masineURadu.FirstOrDefault(m => m.MasinaID == masinaID);
                    if (masinaURadu==null)
                    {
                        masinaURadu = new MasinaURadu() { MasinaID = masinaID, MasinaNaziv = masinaNaziv, Zadaci = new ObservableCollection<Zadatak>() };
                        masineURadu.Add(masinaURadu);
                        canGoLeft = false; // Zadatak koji se dodaje masini bice prvi
                    }
                    else
                    {
                        canGoLeft =  lastStatusNaloga==statusNaloga && lastHitno==hitno;
                    }

                    lastStatusNaloga = statusNaloga;
                    lastHitno = hitno;

                    // Radni nalog koji se dodaje jednoj ili na vise masina
                    NalogURadu nalogURadu = (from m in masineURadu
                                            from z in m.Zadaci
                                            where z.NalogURadu.RadniNalogID == radniNalogID
                                            select z.NalogURadu).FirstOrDefault();
                    if (nalogURadu==null)
                    {
                        nalogURadu = new NalogURadu()
                        {
                            RadniNalogID = radniNalogID,
                            Cilj = cilj,
                            Hitno = hitno,
                            StatusNaloga = statusNaloga,
                            ArtikalID = artikalID,
                            ArtikalSifra = artikalSifra,
                            ArtikalNaziv = artikalNaziv,
                            ArtikalVelicina = artikalVelicina,
                            ArtikalDezen = artikalDezen,
                            Slika1 = slika1,
                            Slika2 = slika2,
                            Slika3 = slika3,
                            PutanjaFajla = putanjaFajla,
                            Podsetnik = podsetnik
                        };

                        nalogURadu.Uradjeno = GetPredatoByRadniNalog(nalogURadu.RadniNalogID);
                        //nalogURadu.Fali = nalogURadu.Cilj - nalogURadu.Uradjeno; // Ovo se automatski racuna u samom modelu
                    }

                    // Zadaci
                    Zadatak zadatak = new Zadatak()
                    {
                        ID = id,
                        MasinaID = masinaID,
                        StatusMasine = statusMasine,
                        Hitno = hitno,
                        Redosled = redosled,
                        NalogURadu = nalogURadu,
                        DatumPredajnice = _datum,
                        CanGoLeft = canGoLeft
                    };

                    Predajnica p;
                    p = predajniceNaDan.FirstOrDefault(p => p.Smena == 1 && p.MasinaID == masinaID && p.RadniNalogID == nalogURadu.RadniNalogID);
                    if (p!=null)
                    {
                        zadatak.PrvaSmenaPredajnicaID = p.ID;
                        zadatak.PrvaSmenaRadnikID = p.LiceID;
                        zadatak.PrvaSmena1kl = p.Kolicina;
                        zadatak.PrvaSmena2kl = p.DrugaKl;
                    }

                    p = predajniceNaDan.FirstOrDefault(p => p.Smena == 2 && p.MasinaID == masinaID && p.RadniNalogID == nalogURadu.RadniNalogID);
                    if (p != null)
                    {
                        zadatak.DrugaSmenaPredajnicaID = p.ID;
                        zadatak.DrugaSmenaRadnikID = p.LiceID;
                        zadatak.DrugaSmena1kl = p.Kolicina;
                        zadatak.DrugaSmena2kl = p.DrugaKl;
                    }

                    p = predajniceNaDan.FirstOrDefault(p => p.Smena == 3 && p.MasinaID == masinaID && p.RadniNalogID == nalogURadu.RadniNalogID);
                    if (p != null)
                    {
                        zadatak.TrecaSmenaPredajnicaID = p.ID;
                        zadatak.TrecaSmenaRadnikID = p.LiceID;
                        zadatak.TrecaSmena1kl = p.Kolicina;
                        zadatak.TrecaSmena2kl = p.DrugaKl;
                    }

                    masinaURadu.Zadaci.Add(zadatak); 

                }
            }


            return masineURadu;
        }

        public IEnumerable<Masina> GetAllMasine()
        {
            List<Masina> lista = new List<Masina>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM masine";

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add(new Masina()
                    {
                        ID = (long)dr["id"],
                        Naziv = (string)dr["naziv"],
                        Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                        Slika = (dr["slika"] == DBNull.Value) ? string.Empty : (string)dr["slika"],
                        RadniNalogID = (long)dr["radninalogid"],
                        Aktivan = (long)dr["aktivan"] ==1
                    }); ;
                    
                }
            }

            return lista;
        }

        public long InsertOrUpdateMasina(Masina masina)
        {
            bool noviZapis = masina.ID == 0;
            if (noviZapis)
            {
                return this.InsertMasina(masina);
            }
            else
            {
                return this.UpdateMasina(masina) ? masina.ID : 0;
            }
        }

        private bool UpdateMasina(Masina masina)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", masina.ID);
            cmd.Parameters.AddWithValue("@naziv", masina.Naziv);
            cmd.Parameters.AddWithValue("@opis", masina.Opis);
            cmd.Parameters.AddWithValue("@slika", masina.Slika);
            cmd.Parameters.AddWithValue("@aktivan", masina.Aktivan);

            cmd.CommandText = "UPDATE masine SET naziv=@naziv, opis=@opis, slika=@slika, aktivan=@aktivan WHERE id=@id";

            cmd.ExecuteNonQuery();

            return true;
        }

        private long InsertMasina(Masina masina)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@naziv", masina.Naziv);
            cmd.Parameters.AddWithValue("@opis", masina.Opis);
            cmd.Parameters.AddWithValue("@slika", masina.Slika);
            cmd.Parameters.AddWithValue("@aktivan", masina.Aktivan);

            cmd.CommandText = "INSERT INTO masine (naziv,opis,slika,aktivan) VALUES (@naziv,@opis,@slika,@aktivan)";

            if (cmd.ExecuteNonQuery() == 1)
                return con.LastInsertRowId;
            else
                return masina.ID;
        }


        public bool DeleteMasina(long id)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.CommandText = "DELETE FROM masine WHERE id=@id";
            try
            {
                  return cmd.ExecuteNonQuery() == 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.Source);
            }
            return false;
       }

        public Masina GetByNazivMasina(string naziv)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM Masine WHERE naziv=@naziv";
            cmd.Parameters.AddWithValue("@naziv", naziv);

            using SQLiteDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

            if (dr.HasRows)
            {
                dr.Read();


                return new Masina()
                {
                    ID = (long)dr["id"],
                    Naziv = (string)dr["naziv"],
                    Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                    Slika = (dr["slika"] == DBNull.Value) ? string.Empty : (string)dr["slika"],
                    Aktivan = (long)dr["aktivan"]==1,
                    VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                };
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<AngazovanaMasina> GetAngazovaneMasinePoRadnomNalogu(long radniNalogID)
        {
            List<AngazovanaMasina> lista = new List<AngazovanaMasina>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM AngazovaneMasine WHERE RadniNalogID=@radninalogID";
            cmd.Parameters.AddWithValue("@radninalogID", radniNalogID);

            try
            {
                using SQLiteDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {

                    while (dr.Read())
                    {

                        lista.Add(new AngazovanaMasina()
                        {
                            ID = (long)dr["id"],
                            RadniNalogID = (long)dr["radninalogid"],
                            MasinaID = (long)dr["MasinaID"],
                            Status = (byte)(long)dr["status"],
                            Redosled = (int)(long)dr["redosled"]
                        }); ;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.Source);
            }

            return lista;
        }

        public bool InsertOrUpdateAngazovaneMasine(long radniNalogID, IEnumerable<long> noveMasineID)
        {
            if (radniNalogID == 0)
            {
                MessageBox.Show("radninalogID ne moze biti 0. Poruka iz procedure InsertOrUpdateAngazovaneMasine", "Nije moguce snimiti angazovane masine");
                return false;
            }

            //1-Iz baze ucitavam sve angazovane masine za trazeni radni nalog
            IEnumerable<AngazovanaMasina> izBaze = GetAngazovaneMasinePoRadnomNalogu(radniNalogID);

            //2-Brisem uklonjene masine (iz kolekcije izBaze sve zapise koji nemaju MasinaID da se nalazi u kolekciji masineID)
            var uklonjeneMasine = from am in izBaze
                                    where am.RadniNalogID == radniNalogID && !noveMasineID.Contains(am.MasinaID)
                                    select am.ID;

            foreach (long uklonjenID in uklonjeneMasine)
            {
                using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
                con.Open();
                using var cmd = new SQLiteCommand(con);
                cmd.Parameters.AddWithValue("@id", uklonjenID);
                cmd.CommandText = "DELETE FROM angazovanemasine WHERE id=@id";
                cmd.ExecuteNonQuery();
            }

            //3-Dodajem nove angazovane masine
            var dodateMasine = from m in noveMasineID
                                 where !izBaze.Any(b => b.MasinaID == m)
                                 select m;

            foreach (long dodataMasinaID in dodateMasine)
            {
                insertAngazovanaMasina(radniNalogID, dodataMasinaID);
            }

            return true;
        }

        private long insertAngazovanaMasina(long radniNalogID, long masinaID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@radninalogid", radniNalogID);
            cmd.Parameters.AddWithValue("@masinaid", masinaID);

            cmd.CommandText = "INSERT INTO angazovanemasine (radninalogid,masinaid) VALUES (@radninalogid,@masinaid)";

            if (cmd.ExecuteNonQuery() == 1)
                return con.LastInsertRowId;
            else
                return 0;

        }


        public bool MasinaImaZavisneZapise(long id)
        {
            // TODO
            return false;
        }

        #endregion // Masine

        #region Artikli
        public Artikal GetArtikal(long artikalID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM Artikli WHERE ID=@id";
            cmd.Parameters.AddWithValue("@id", artikalID);

            using SQLiteDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

            if (dr.HasRows)
            {
                dr.Read();


                return new Artikal()
                {
                    ID = (long)dr["id"],
                    Sifra = (string)dr["sifra"],
                    Naziv = (string)dr["naziv"],
                    Jm = (string)dr["jm"],
                    Slika = (dr["slika"] == DBNull.Value) ? string.Empty : (string)dr["slika"],
                    BarKod = (dr["barkod"] == DBNull.Value) ? string.Empty : (string)dr["barkod"],
                    VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                };
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Artikal> GetAllArtikli()
        {
            List<Artikal> lista = new List<Artikal>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM artikli";

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {


                    lista.Add(new Artikal()
                    {
                        ID = (long)dr["id"],
                        Sifra = (string)dr["sifra"],
                        Naziv = (string)dr["naziv"],
                        Jm = (string)dr["jm"],
                        Slika = (dr["slika"] == DBNull.Value) ? string.Empty : (string)dr["slika"],
                        BarKod = (dr["barkod"] == DBNull.Value) ? string.Empty : (string)dr["barkod"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    }); ;

                }
            }

            return lista;
        }


        public long InsertOrUpdateArtikal(Artikal artikal)
        {
            bool noviZapis = artikal.ID == 0;
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@sifra", artikal.Sifra);
            cmd.Parameters.AddWithValue("@naziv", artikal.Naziv);
            cmd.Parameters.AddWithValue("@jm", artikal.Jm);
            cmd.Parameters.AddWithValue("@slika", artikal.Slika);
            cmd.Parameters.AddWithValue("@barkod", artikal.BarKod);

            if (noviZapis)
            {
                cmd.CommandText = "INSERT INTO artikli (sifra,naziv,jm,slika,barkod) VALUES (@sifra,@naziv,@jm,@slika,@barkod)";
                long odgovor = 0;
                try
                {
                    odgovor = cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Šifra artikla \"{artikal.Sifra}\" već postoji u bazi."),
                            "Nisu dozvoljene duple šifre.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

                if (odgovor == 1)
                    return con.LastInsertRowId;
            }
            else
            {
                cmd.Parameters.AddWithValue("@id", artikal.ID);
                cmd.CommandText = "UPDATE artikli SET sifra=@sifra, naziv=@naziv, jm=@jm, slika=@slika, barkod=@barkod WHERE id=@id";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Šifra artikla \"{artikal.Sifra}\" već postoji u bazi."),
                            "Nisu dozvoljene duple šifre.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

            }
            return artikal.ID;
        }

        public bool IzbrisiArtikal(long artikalID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@artikalid", artikalID);

            bool izbrisan = false;
            // Otvaram transakciju
            cmd.CommandText = "begin";
            cmd.ExecuteNonQuery();

            try
            {
                cmd.CommandText = "DELETE FROM velicineartikla WHERE ArtikalID = @artikalid";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "DELETE FROM DezeniArtikla WHERE ArtikalID = @artikalid";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "DELETE FROM artikli WHERE id=@artikalid";
                izbrisan = cmd.ExecuteNonQuery()==1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format($"nMessage: {ex.Message}"),"Greska",MessageBoxButton.OK,MessageBoxImage.Error,MessageBoxResult.OK);
            }

            // Kraj transakcije
            cmd.CommandText = "end";
            cmd.ExecuteNonQuery();

            return izbrisan;
        }

        #endregion

        #region DezeniArtikala

        public long InsertOrUpdateDezenArtikla(DezenArtikla dezenArtikla)
        {
            bool noviZapis = dezenArtikla.ID == 0;
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@artikalid", dezenArtikla.ArtikalID);
            cmd.Parameters.AddWithValue("@naziv", dezenArtikla.Naziv);
            cmd.Parameters.AddWithValue("@opis", dezenArtikla.Opis);
            cmd.Parameters.AddWithValue("@putanja", dezenArtikla.Putanja);
            cmd.Parameters.AddWithValue("@slika1", dezenArtikla.Slika1);
            cmd.Parameters.AddWithValue("@slika2", dezenArtikla.Slika2);
            cmd.Parameters.AddWithValue("@slika3", dezenArtikla.Slika3);
            cmd.Parameters.AddWithValue("@aktivan", dezenArtikla.Aktivan);

            if (noviZapis)
            {
                cmd.CommandText = "INSERT INTO dezeniartikla (artikalid,naziv,opis,slika1,slika2,slika3,aktivan) VALUES (@artikalid,@naziv,@opis,@slika1,@slika2,@slika3,@aktivan)";
                long odgovor = 0;
                try
                {
                    odgovor = cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Naziv dezen \"{dezenArtikla.Naziv}\" za izabrani artikal već postoji u bazi."),
                            "Nisu dozvoljene dupli nazivi dezena za isti artikal.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

                if (odgovor == 1)
                    return con.LastInsertRowId;
            }
            else
            {
                cmd.Parameters.AddWithValue("@id", dezenArtikla.ID);
                cmd.CommandText = "UPDATE dezeniartikla SET artikalid=@artikalid, naziv=@naziv,  opis=@opis, putanja=@putanja, slika1=@slika1 " +
                    ",slika2=@slika2, slika3=@slika3, aktivan=@aktivan WHERE id=@id";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Naziv dezen \"{dezenArtikla.Naziv}\" za izabrani artikal već postoji u bazi."),
                            "Nisu dozvoljene dupli nazivi dezena za isti artikal.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

            }
            return dezenArtikla.ID;
        }

        public IEnumerable<DezenArtikla> GetDezeniArtikla(long artikalID)
        {
            List<DezenArtikla> lista = new List<DezenArtikla>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM DezeniArtikla WHERE ArtikalID=@artikalID";
            cmd.Parameters.AddWithValue("@artikalID", artikalID);

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add(
                        new DezenArtikla() 
                        { 
                            ID = (long)dr["id"], 
                            ArtikalID = (long)dr["artikalid"], 
                            Naziv = (string)dr["naziv"],
                            Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                            Putanja = (dr["putanja"] == DBNull.Value) ? string.Empty : (string)dr["putanja"],
                            Slika1 = (dr["slika1"] == DBNull.Value) ? string.Empty : (string)dr["slika1"],
                            Slika2 = (dr["slika2"] == DBNull.Value) ? string.Empty : (string)dr["slika2"],
                            Slika3 = (dr["slika3"] == DBNull.Value) ? string.Empty : (string)dr["slika3"],
                            VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                        }
                    );
                }
            }

            return lista;
        }

        public DezenArtikla GetDezenArtikla(long dezenArtiklaID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM DezeniArtikla WHERE ID=@id";
            cmd.Parameters.AddWithValue("@id", dezenArtiklaID);

            try
            {
                using SQLiteDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                if (dr.HasRows)
                {
                    dr.Read();

                    return new DezenArtikla()
                    {
                        ID = (long)dr["id"],
                        ArtikalID = (long)dr["artikalid"],
                        Naziv = (string)dr["naziv"],
                        Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                        Putanja = (dr["putanja"] == DBNull.Value) ? string.Empty : (string)dr["putanja"],
                        Slika1 = (dr["slika1"] == DBNull.Value) ? string.Empty : (string)dr["slika1"],
                        Slika2 = (dr["slika2"] == DBNull.Value) ? string.Empty : (string)dr["slika2"],
                        Slika3 = (dr["slika3"] == DBNull.Value) ? string.Empty : (string)dr["slika3"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                return null;
            }
        }

        public bool IzbrisiDezenArtikla(long id)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.CommandText = "DELETE FROM DezeniArtikla WHERE id=@id";
            return cmd.ExecuteNonQuery() == 1;
        }

        #endregion //DezeniArtikala

        #region Velicine

        public long InsertOrUpdateVelicina(Velicina velicina)
        {
            if (string.IsNullOrWhiteSpace(velicina.Oznaka))
            {
                return 0;
            }

            bool noviZapis = velicina.ID == 0;
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@oznaka", velicina.Oznaka);

            if (noviZapis)
            {
                cmd.CommandText = "INSERT INTO velicine (oznaka) VALUES (@oznaka)";

                long odgovor = 0;
                try
                {
                    odgovor = cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode==19)
                    {
                        MessageBox.Show(String.Format($"Veličina sa oznakom \"{velicina.Oznaka}\" već postoji u bazi."),
                            "Nisu dozvoljeni dupli unosi",MessageBoxButton.OK,MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

                if (odgovor == 1)
                    return con.LastInsertRowId;
            }
            else
            {
                cmd.Parameters.AddWithValue("@id", velicina.ID);
                cmd.CommandText = "UPDATE velicine SET oznaka=@oznaka WHERE id=@id";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19)
                    {
                        MessageBox.Show(String.Format($"Veličina sa oznakom \"{velicina.Oznaka}\" već postoji u bazi."),
                            "Nisu dozvoljeni dupli unosi", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }
            }
            return velicina.ID;
        }

        public IEnumerable<Velicina> GetAllVelicine()
        {
            List<Velicina> lista = new List<Velicina>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM velicine ORDER BY oznaka";

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {


                    lista.Add(new Velicina()
                    {
                        ID = (long)dr["id"],
                        Oznaka = (string)dr["oznaka"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    }); ;

                }
            }

            return lista;
        }

        public IEnumerable<Velicina> GetVelicine(long artikalID)
        {
            List<Velicina> lista = new List<Velicina>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT Velicine.* FROM Velicine INNER JOIN VelicineArtikla ON Velicine.ID=VelicineArtikla.VelicinaID WHERE VelicineArtikla.ArtikalID=@artikalID ORDER BY Velicine.Oznaka";
            cmd.Parameters.AddWithValue("@artikalID", artikalID);

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add(new Velicina()
                    {
                        ID = (long)dr["id"],
                        Oznaka = (string)dr["oznaka"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    });
                }
            }

            return lista;
        }


        public bool IzbrisiVelicinu(long id)
        {
            // Provera da li postoje artikli sa unetom velicimom
            IEnumerable<string> artikliSaVelicinom = getSifreArtikalaByVelicinaId(id);
            if (artikliSaVelicinom.Count()>0)
            {
                MessageBox.Show("Nije moguce izbrisati veličinu zato što postoje artikli kojima je dodeljena ova veličina: " + string.Join(", ", artikliSaVelicinom),"",MessageBoxButton.OK,MessageBoxImage.Exclamation);
                return false;
            }

            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.CommandText = "DELETE FROM velicine WHERE id=@id";
            try
            {
                  return cmd.ExecuteNonQuery() == 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }


        #endregion //Velicine

        #region VelicineArtikla
        public IEnumerable<VelicinaArtikla> GetFromVelicineArtikla(long artikalID)
        {
            List<VelicinaArtikla> lista = new List<VelicinaArtikla>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM VelicineArtikla WHERE VelicineArtikla.ArtikalID=@artikalID";
            cmd.Parameters.AddWithValue("@artikalID", artikalID);

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add(new VelicinaArtikla()
                    {
                        ID = (long)dr["id"],
                        ArtikalID = (long)dr["artikalid"],
                        VelicinaID = (long)dr["velicinaid"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    }); ;
                }
            }

            return lista;
        }

        public bool InsertOrUpdateVelicineArtikla(long artikalID, IEnumerable<long> noveVelicineID)
        {
            if (artikalID==0)
            {
                MessageBox.Show("artikalID ne moze biti 0. Poruka iz procedure InsertOrUpdateVelicineArtikla", "Nije moguce snimiti velicine");
                return false;
            }

            //1-Iz baze ucitavam sve velicine za trazeni artikal
            IEnumerable<VelicinaArtikla> izBaze = GetFromVelicineArtikla(artikalID);

            //2-Brisem uklonjene velicine (iz kolekcije izBaze sve zapise koji nemaju VelicinaID da se nalazi u kolekciji velicineID)
            var uklonjeneVelicine = from va in izBaze
                                    where va.ArtikalID == artikalID && !noveVelicineID.Contains(va.VelicinaID)
                                    select va.ID;

            foreach (long uklonjenID in uklonjeneVelicine)
            {
                using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
                con.Open();
                using var cmd = new SQLiteCommand(con);
                cmd.Parameters.AddWithValue("@id", uklonjenID);
                cmd.CommandText = "DELETE FROM velicineartikla WHERE id=@id";
                cmd.ExecuteNonQuery();
            }

            //3-Dodajem nove velicine
            var dodateVelicine = from n in noveVelicineID
                                 where !izBaze.Any(b => b.VelicinaID == n)
                                 select n;

            foreach (long dodataVelicinaID in dodateVelicine)
            {
                insertVelicinaArtikla(artikalID, dodataVelicinaID);
            }

            return true;
        }

        private long insertVelicinaArtikla(long artikalID, long velicinaID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@artikalid", artikalID);
            cmd.Parameters.AddWithValue("@velicinaid", velicinaID);

            cmd.CommandText = "INSERT INTO velicineartikla (artikalid,velicinaid) VALUES (@artikalid,@velicinaid)";

            if (cmd.ExecuteNonQuery() == 1)
                return con.LastInsertRowId;
            else
                return 0;

        }

        private IEnumerable<string> getSifreArtikalaByVelicinaId(long velicinaId)
        {
            List<string> lista = new List<string>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT Artikli.Sifra FROM VelicineArtikla INNER JOIN Artikli ON VelicineArtikla.ArtikalID = Artikli.ID WHERE VelicineArtikla.VelicinaID = @velicinaId";
            cmd.Parameters.AddWithValue("@velicinaId", velicinaId);

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add((string)dr[0]); ;
                }
            }

            return lista;
        }

        #endregion

        #region RadniNalozi

        public long InsertOrUpdateRadniNalog(RadniNalog radniNalog)
        {
            bool noviZapis = radniNalog.ID == 0;
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@artikalid", radniNalog.ArtikalID);
            cmd.Parameters.AddWithValue("@dezenartiklaid", radniNalog.DezenArtiklaID);
            cmd.Parameters.AddWithValue("@velicinaid", radniNalog.VelicinaID);
            cmd.Parameters.AddWithValue("@cilj", radniNalog.Cilj);
            cmd.Parameters.AddWithValue("@opis", radniNalog.Opis);
            cmd.Parameters.AddWithValue("@podsetnik", radniNalog.Podsetnik);
            cmd.Parameters.AddWithValue("@hitno", radniNalog.Hitno);
            cmd.Parameters.AddWithValue("@status", radniNalog.Status);

            if (noviZapis)
            {
                cmd.CommandText = "INSERT INTO radninalozi "+
                    "( artikalid, dezenartiklaid, velicinaid, cilj, opis, podsetnik, hitno, status) VALUES "+
                    "(@artikalid,@dezenartiklaid,@velicinaid,@cilj,@opis,@podsetnik,@hitno,@status)";
                long odgovor = 0;
                try
                {
                    odgovor = cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

                if (odgovor == 1)
                    return con.LastInsertRowId;
            }
            else
            {
                cmd.Parameters.AddWithValue("@id", radniNalog.ID);
                cmd.CommandText = "UPDATE radninalozi SET artikalid=@artikalid,dezenartiklaid=@dezenartiklaid," +
                    "velicinaid=@velicinaid,cilj=@cilj,opis=@opis,podsetnik=@podsetnik,hitno=@hitno,status=@status WHERE id=@id";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }
            }
            return radniNalog.ID;
        }

        public RadniNalog GetRadniNalog(long radniNalogID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT id,artikalid,dezenartiklaid,velicinaid,cilj,napravljeno,opis,podsetnik,hitno,status,vremeunosa FROM RadniNalozi WHERE ID=@id";
            cmd.Parameters.AddWithValue("@id", radniNalogID);

            try
            {
                using SQLiteDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                if (dr.HasRows)
                {
                    dr.Read();

                    return new RadniNalog()
                    {
                        ID = (long)dr["id"],
                        ArtikalID = (long)dr["artikalid"],
                        DezenArtiklaID = (long)dr["dezenartiklaid"],
                        VelicinaID = (long)dr["velicinaid"],
                        Cilj = (long)dr["cilj"],
                        Napravljeno = (long)dr["napravljeno"],
                        Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                        Podsetnik = (dr["podsetnik"] == DBNull.Value) ? string.Empty : (string)dr["podsetnik"],
                        Hitno = (long)dr["hitno"] == 1,
                        Status = (byte)(long)dr["status"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite(dr["vremeunosa"].ToString())
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public bool DeleteRadniNalog(long radniNalogID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", radniNalogID);

            bool izbrisan = false;
            // Otvaram transakciju
            cmd.CommandText = "begin";
            cmd.ExecuteNonQuery();

            try
            {
                cmd.CommandText = "DELETE FROM AngazovaneMasine WHERE RadniNalogID=@id";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "DELETE FROM radninalozi WHERE id=@id";
                izbrisan = cmd.ExecuteNonQuery() == 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format($"nMessage: {ex.Message}"), "Greska", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }

            // Kraj transakcije
            cmd.CommandText = "end";
            cmd.ExecuteNonQuery();

            return izbrisan;
        }

        public bool ZakljuciRadniNalog(long radniNalogID)
        {
            return PostaviStatusRadnogNaloga(radniNalogID, StatusRadnogNaloga.Zatvoren);
        }

        public bool AktivirajRadniNalog(long radniNalogID)
        {
            return PostaviStatusRadnogNaloga(radniNalogID, StatusRadnogNaloga.Aktivan);
        }

        private bool PostaviStatusRadnogNaloga(long radniNalogID, byte status)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@id", radniNalogID);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.CommandText = "UPDATE RadniNalozi SET status=@status WHERE id=@id";

            return IzvrsiCmdNonQuery(cmd, 1) > 0;
        }

        public Istorija GetNextIstorija(long masinaID, long trenutniRnID, Kretanje kretanje)
        {
            if (masinaID==0) { return null; }

            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            bool isNotZadnji=true, isNotPrvi = true;
            string strTrenutni = string.Empty;
            string order = " DESC ";
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@masinaid", masinaID);
            if (trenutniRnID!=0)
            {
                cmd.Parameters.AddWithValue("@trenutnirnid", trenutniRnID);
                strTrenutni = " AND RadniNalozi.ID " + (kretanje == Kretanje.Nazad ? "<" : ">") + "@trenutnirnid ";
                if (kretanje==Kretanje.Napred)
                {
                    order = " ASC ";
                }
            }
            cmd.CommandText = "SELECT Predajnice.Kolicina, Predajnice.Smena, Predajnice.DrugaKl, Predajnice.Datum, " +
                           "RadniNalozi.ID as RadniNalogID, RadniNalozi.ArtikalID," +
                           "Artikli.Naziv as ArtikalNaziv, Velicine.Oznaka as ArtikalVelicina, " +
                           "DezeniArtikla.Naziv as ArtikalDezen, DezeniArtikla.Slika1, " +
                           "Lica.Ime, Lica.Prezime " +
                           "FROM Lica " +
                           "INNER JOIN Predajnice ON Predajnice.LiceID = Lica.ID " +
                           "INNER JOIN RadniNalozi ON RadniNalozi.ID = Predajnice.RadniNalogID " +
                           "INNER JOIN Artikli ON RadniNalozi.ArtikalID = Artikli.ID " +
                           "INNER JOIN Velicine ON RadniNalozi.VelicinaID = Velicine.ID " +
                           "INNER JOIN DezeniArtikla ON RadniNalozi.DezenArtiklaID = DezeniArtikla.ID " +
                           "WHERE Predajnice.MasinaID = @masinaid AND RadniNalozi.Status = 9 " +
                           strTrenutni +
                           "GROUP BY RadniNalozi.ID " +
                           "ORDER BY Predajnice.Datum"+order+", Predajnice.ID"+order+"LIMIT 2";

            try
            {
                using SQLiteDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    Istorija istorija = new Istorija();
                    int brojReda = 0;
                    while (dr.Read())
                    {
                        brojReda++;
                        if (brojReda == 1)
                        {
                            string ime = (dr["ime"] == DBNull.Value) ? string.Empty : (string)dr["ime"];
                            string prezime = (dr["prezime"] == DBNull.Value) ? string.Empty : (string)dr["prezime"];

                            istorija.RadniNalogID = (long)dr["radninalogid"];
                            istorija.ArtikalNaziv = (dr["artikalnaziv"] == DBNull.Value) ? string.Empty : (string)dr["artikalnaziv"];
                            istorija.ArtikalVelicina = (dr["artikalvelicina"] == DBNull.Value) ? string.Empty : (string)dr["artikalvelicina"];
                            istorija.ArtikalDezen = (dr["artikaldezen"] == DBNull.Value) ? string.Empty : (string)dr["artikaldezen"];
                            istorija.Slika1 = (dr["slika1"] == DBNull.Value) ? string.Empty : (string)dr["slika1"];
                            istorija.Kolicina = (long)dr["kolicina"];
                            istorija.DrugaKl = (long)dr["drugakl"];
                            istorija.Smena = (byte)(long)dr["smena"];
                            istorija.Radnik = ime + " " + prezime;
                            istorija.Datum = Helper.ConvertToDateTimeFromSqLite((string)dr["datum"]);
                        }
                    };

                    if (trenutniRnID == 0)
                    {
                        isNotZadnji = false;
                        isNotPrvi = brojReda>1;
                    }
                    else if (kretanje == Kretanje.Napred)
                    {
                        isNotPrvi = true;
                        isNotZadnji = brojReda > 1;
                    }
                    else if (kretanje == Kretanje.Nazad)
                    {
                        isNotPrvi = brojReda > 1;
                        isNotZadnji = true;
                    }

                    istorija.IsNotPrvi = isNotPrvi;
                    istorija.IsNotZadnji = isNotZadnji;

                    return istorija;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                return null;
            }
        }

        public bool SetRedosledAngazovaneMasine(long angazovanaMasinaID, int redosled)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@amid", angazovanaMasinaID);
            cmd.Parameters.AddWithValue("@redosled", redosled);
            cmd.CommandText = "UPDATE AngazovaneMasine SET redosled=@redosled WHERE id=@amid";

            return IzvrsiCmdNonQuery(cmd) > 0;
        }
        #endregion

        #region Lica

        public IEnumerable<Lice> GetAllAktivnaLica()
        {
            List<Lice> lista = new List<Lice>();
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM Lica WHERE Aktivan=1";

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {


                    lista.Add(new Lice()
                    {
                        ID = (long)dr["id"],
                        Ime = (string)dr["ime"],
                        Prezime = (string)dr["prezime"],
                        RadnoMesto = (string)dr["radnomesto"],
                        Aktivan = (long)dr["aktivan"]==1,
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    }); ;

                }
            }

            return lista;
        }

        #endregion

        #region Predajnice

        public long InsertOrUpdatePredajnica(Predajnica predajnica, string kolona = "")
        {
            bool noviZapis = predajnica.ID == 0;
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.AddWithValue("@radninalogid", predajnica.RadniNalogID);
            cmd.Parameters.AddWithValue("@masinaid", predajnica.MasinaID);
            cmd.Parameters.AddWithValue("@datum", predajnica.Datum);
            cmd.Parameters.AddWithValue("@liceid", predajnica.LiceID);
            cmd.Parameters.AddWithValue("@smena", predajnica.Smena);
            cmd.Parameters.AddWithValue("@kolicina", predajnica.Kolicina);
            cmd.Parameters.AddWithValue("@drugakl", predajnica.DrugaKl);

            if (noviZapis)
            {
                cmd.CommandText = "INSERT INTO predajnice (radninalogid,masinaid,datum,liceid,smena,kolicina,drugakl)" +
                    " VALUES (@radninalogid,@masinaid,@datum,@liceid,@smena,@kolicina,@drugakl)";
                long odgovor = 0;
                try
                {
                    odgovor = cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                  MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                }
                catch (Exception ex)
                {

                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

                if (odgovor == 1)
                    return con.LastInsertRowId;
            }
            else
            {
                cmd.Parameters.AddWithValue("@id", predajnica.ID);

                string ctInsert;
                if (string.IsNullOrEmpty(kolona))
                    { ctInsert = "radninalogid=@radninalogid,masinaid=@masinaid,datum=@datum,liceid=@liceid,smena=@smena,kolicina=@kolicina,drugakl=@drugakl"; }
                else
                    { ctInsert = kolona + "=@" + kolona.ToLower(); }

                cmd.CommandText = "UPDATE predajnice SET "+ctInsert+" WHERE id=@id";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(String.Format($"ErrorCode: {ex.ErrorCode}\nMessage: {ex.Message}"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format($"nMessage: {ex.Message}"));
                }

            }
            return predajnica.ID;
        }

        public IEnumerable<Predajnica> GetAllPredajniceOnDate(DateTime dateTime)
        {
            //List<Predajnica> lista = new List<Predajnica>();
            //using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            //con.Open();

            //using var cmd = new SQLiteCommand(con);
            //cmd.CommandText = "SELECT * FROM Predajnice WHERE Datum=@datum";
            //cmd.Parameters.AddWithValue("@datum", dateTime.Date);

            //using SQLiteDataReader dr = cmd.ExecuteReader();

            //if (dr.HasRows)
            //{
            //    while (dr.Read())
            //    {


            //        lista.Add(new Predajnica()
            //        {
            //            ID = (long)dr["id"],
            //            RadniNalogID = (long)dr["radninalogid"],
            //            MasinaID = (long)dr["masinaid"],
            //            Datum = Helper.ConvertToDateTimeFromSqLite((string)dr["datum"]),
            //            LiceID = (long)dr["liceid"],
            //            Smena = (byte)(long)dr["smena"],
            //            Kolicina = (long)dr["kolicina"],
            //            DrugaKl = (long)dr["drugakl"],
            //            VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
            //        }); ;
            //    }
            //}

            return GetPredajnice(dateTime: dateTime);
        }

        public IEnumerable<Predajnica> GetPredajniceByMasinaAndNalogOnDate(long masinaID, long radniNalogID, DateTime dateTime)
        {
            return GetPredajnice(masinaID, radniNalogID, dateTime);
        }

        private IEnumerable<Predajnica> GetPredajnice(long masinaID=0, long radniNalogID=0, DateTime? dateTime=null)
        {
            List<Predajnica> lista = new List<Predajnica>();

            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);

            string whereSql = string.Empty;
            string temp = string.Empty;
            if (radniNalogID != 0)
            {
                cmd.Parameters.AddWithValue("@radninalogid", radniNalogID);
                if (!string.IsNullOrEmpty(whereSql)) { whereSql += " AND "; }
                whereSql += "RadniNalogID=@radninalogid";
            }

            if (masinaID!=0) 
            { 
                cmd.Parameters.AddWithValue("@masinaid", masinaID);
                if (!string.IsNullOrEmpty(whereSql)) { whereSql += " AND "; }
                whereSql += "MasinaID=@masinaid";
            }

            if (dateTime!=null)
            {
                cmd.Parameters.AddWithValue("@datum", ((DateTime)dateTime).Date);
                if (!string.IsNullOrEmpty(whereSql)) { whereSql += " AND "; }
                whereSql += "Datum=@datum";
            }

            if (!string.IsNullOrEmpty(whereSql))
            {
                whereSql = " WHERE " + whereSql;
            }

            cmd.CommandText = "SELECT * FROM Predajnice" + whereSql;

            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {

                    lista.Add(new Predajnica()
                    {
                        ID = (long)dr["id"],
                        RadniNalogID = (long)dr["radninalogid"],
                        MasinaID = (long)dr["masinaid"],
                        Datum = Helper.ConvertToDateTimeFromSqLite((string)dr["datum"]),
                        LiceID = (long)dr["liceid"],
                        Smena = (byte)(long)dr["smena"],
                        Kolicina = (long)dr["kolicina"],
                        DrugaKl = (long)dr["drugakl"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr["vremeunosa"])
                    }); ;
                }
            }

            return lista;
        }

        public long GetPredatoByRadniNalog (long radniNalogID)
        {
            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT SUM(kolicina) FROM Predajnice WHERE RadniNalogID=@radninalogid";
            cmd.Parameters.AddWithValue("@radninalogid", radniNalogID);

            object o = cmd.ExecuteScalar();
            return o== System.DBNull.Value ? 0 : (long)o;
        }

        #endregion //Predajnica

        #region Business
        public RadniNalogPregled GetRadniNalogPregled(long radniNalogID)
        {
            if (radniNalogID == 0)
            {
                return null;
            }

            RadniNalogPregled radniNalogPregled;

            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT rn.id,rn.artikalid,rn.dezenartiklaid,rn.velicinaid,rn.cilj,rn.napravljeno,rn.opis,rn.podsetnik,rn.hitno,status,rn.vremeunosa," +
            "a.Sifra as ArtikalSifra, a.Naziv as ArtikalNaziv, da.Naziv as DezenNaziv, da.Opis as DezenOpis, da.Putanja as DezenPutanja," +
            "da.Slika1, da.Slika2, da.Slika3, v.Oznaka as VelicinaOznaka" +
            " FROM RadniNalozi rn" +
            " inner join Artikli a on rn.ArtikalID = a.ID" +
            " inner join DezeniArtikla da on a.ID = da.ArtikalID" +
            " inner join Velicine v on rn.VelicinaID = v.ID WHERE rn.ID=@id";
            cmd.Parameters.AddWithValue("@id", radniNalogID);

            try
            {
                using SQLiteDataReader dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                if (dr.HasRows)
                {
                    dr.Read();

                    radniNalogPregled = new RadniNalogPregled()
                    {
                        ID = (long)dr["id"],
                        ArtikalID = (long)dr["artikalid"],
                        DezenArtiklaID = (long)dr["dezenartiklaid"],
                        VelicinaID = (long)dr["velicinaid"],
                        Cilj = (long)dr["cilj"],
                        Napravljeno = (long)dr["napravljeno"],
                        Opis = (dr["opis"] == DBNull.Value) ? string.Empty : (string)dr["opis"],
                        Podsetnik = (dr["podsetnik"] == DBNull.Value) ? string.Empty : (string)dr["podsetnik"],
                        Hitno = (long)dr["hitno"] == 1,
                        Status = (byte)(long)dr["status"],
                        VremeUnosa = Helper.ConvertToDateTimeFromSqLite(dr["vremeunosa"].ToString()),
                        ArtikalSifra = (dr["artikalsifra"] == DBNull.Value) ? string.Empty : (string)dr["artikalsifra"],
                        ArtikalNaziv = (dr["artikalnaziv"] == DBNull.Value) ? string.Empty : (string)dr["artikalnaziv"],
                        DezenNaziv = (dr["dezennaziv"] == DBNull.Value) ? string.Empty : (string)dr["dezennaziv"],
                        DezenOpis = (dr["dezenopis"] == DBNull.Value) ? string.Empty : (string)dr["dezenopis"],
                        DezenPutanja = (dr["dezenputanja"] == DBNull.Value) ? string.Empty : (string)dr["dezenputanja"],
                        Slika1 = (dr["slika1"] == DBNull.Value) ? string.Empty : (string)dr["slika1"],
                        Slika2 = (dr["slika2"] == DBNull.Value) ? string.Empty : (string)dr["slika2"],
                        Slika3 = (dr["slika3"] == DBNull.Value) ? string.Empty : (string)dr["slika3"],
                        VelicinaOznaka = (dr["velicinaoznaka"] == DBNull.Value) ? string.Empty : (string)dr["velicinaoznaka"]
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

            // Popunjavam listu predajnica
            cmd.CommandText = "SELECT p.*,m.Naziv as MasinaNaziv, l.Ime as LiceIMe, l.Prezime as LicePrezime  FROM Predajnice p" +
                            " inner join Masine m ON p.MasinaID = m.ID" +
                            " inner join Lica l ON p.LiceID = l.ID WHERE p.radninalogid=@radninalogid";
            cmd.Parameters.AddWithValue("@radninalogid", radniNalogID);

            try
            {
                using SQLiteDataReader dr2 = cmd.ExecuteReader();

                if (dr2.HasRows)
                {
                    while (dr2.Read())
                    {
                        PredajnicaPregled pp = new PredajnicaPregled()
                        {
                            ID = (long)dr2["id"],
                            RadniNalogID = (long)dr2["radninalogid"],
                            MasinaID = (long)dr2["masinaid"],
                            Datum = Helper.ConvertToDateTimeFromSqLite((string)dr2["datum"]),
                            LiceID = (long)dr2["liceid"],
                            Smena = (byte)(long)dr2["smena"],
                            Kolicina = (long)dr2["kolicina"],
                            DrugaKl = (long)dr2["drugakl"],
                            VremeUnosa = Helper.ConvertToDateTimeFromSqLite((string)dr2["vremeunosa"]),
                            MasinaNaziv = (dr2["masinanaziv"] == DBNull.Value) ? string.Empty : (string)dr2["masinanaziv"],
                            LiceIme = (dr2["liceime"] == DBNull.Value) ? string.Empty : (string)dr2["liceime"],
                            LicePrezime = (dr2["liceprezime"] == DBNull.Value) ? string.Empty : (string)dr2["liceprezime"],
                        };
                        pp.LicePrezimeIme = pp.LicePrezime + " " + pp.LicePrezime;

                        radniNalogPregled.Predajnice.Add(pp); ;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return radniNalogPregled;
        }

        public IEnumerable<IzvestajStavka> GetStavkeZaIzvestaj(DateTime? datumOd=null, DateTime? datumDo=null)
        {
            List<IzvestajStavka> lista = new List<IzvestajStavka>();

            using var con = new SQLiteConnection(GlobalniKod.ConnectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);

            string whereSql = string.Empty;
            if (datumOd.HasValue && datumDo.HasValue)
            {
                whereSql = " WHERE datum BETWEEN @datumOd and @datumDo ";
                cmd.Parameters.AddWithValue("@datumOd", datumOd);
                cmd.Parameters.AddWithValue("@datumDo", datumDo);
            }
            else if (datumOd.HasValue)
            {
                whereSql = " WHERE datum >= @datumOd ";
                cmd.Parameters.AddWithValue("@datumOd", datumOd);
                cmd.Parameters.AddWithValue("@datumDo", datumDo);
            }
            else if (datumDo.HasValue)
            {
                whereSql = " WHERE datum <= datumDo";
                cmd.Parameters.AddWithValue("@datumDo", datumDo);
            }

            cmd.CommandText = "SELECT a.Sifra, a.Naziv, v.Oznaka as Velicina, da.Naziv as Dezen, rn.ID as RadniNalog, m.Naziv as Masina, p.Smena, p.Kolicina, p.DrugaKl, p.Datum, l.Ime || ' ' || l.Prezime AS Radnik, strftime('%m', p.Datum) as Mesec, strftime('%Y', p.Datum) as Godina " +
                                "FROM Predajnice p " +
                                "INNER JOIN RadniNalozi rn ON rn.ID = p.RadniNalogID " +
                                "INNER JOIN Lica l ON l.ID = p.LiceID " +
                                "INNER JOIN Artikli a ON a.ID = RN.ArtikalID " +
                                "INNER JOIN Velicine v ON v.ID = rn.VelicinaID " +
                                "INNER JOIN DezeniArtikla da ON da.ID = rn.DezenArtiklaID " +
                                "INNER JOIN Masine m ON m.ID = p.MasinaID" +
                                whereSql;
            using SQLiteDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {

                    lista.Add(new IzvestajStavka()
                    {
                        Sifra = (string)dr["sifra"],
                        Naziv = (string)dr["naziv"],
                        Velicina = (string)dr["velicina"],
                        Dezen = (string)dr["dezen"],
                        RadniNalog = (long)dr["radninalog"],
                        Masina = (string)dr["masina"],
                        Smena = (long)dr["smena"],
                        Kolicina = (long)dr["kolicina"],
                        DrugaKl = (long)dr["drugakl"],
                        Datum = Helper.ConvertToDateTimeFromSqLite((string)dr["datum"]),
                        Radnik = (string)dr["radnik"],
                        Mesec = (string)dr["mesec"],
                        Godina = (string)dr["godina"]
                    }); ;

                }
            }
            return lista;
        }

        #endregion

    }
}