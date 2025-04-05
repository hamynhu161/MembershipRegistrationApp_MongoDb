using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jäsenrekisteriohjelma_MongoDb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMongoCollection<Jasen> collection;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMongoDb();            //Luo yhteys MongoDB-tietokantaan
        }

        private void InitializeMongoDb()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("test");
            collection = database.GetCollection<Jasen>("jasenet");
        }

        private async void btn_Talenna_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //luodaan uusi Jasen-olio
                if (!int.TryParse(txbPosti.Text, out int postinumero))
                {
                    MessageBox.Show("Virheellinen postinumero! Anna numero.");
                    return;
                }

                if (!int.TryParse(txbPuhelin.Text, out int puhelin))
                {
                    MessageBox.Show("Virheellinen puhelinnumero! Anna numero.");
                    return;
                }

                var uusiJasen = new Jasen
                {
                    Etunimi = txbEtunimi.Text,
                    Sukunimi = txbSukunimi.Text,
                    Osoite = txbOsoite.Text,
                    Postinumero = postinumero,
                    Puhelin = puhelin,
                    Sahkoposti = txbEmail.Text,
                    JasenydenAlkuPvm = datepicker_Aika.SelectedDate,
                };


                //lisätään MongoDB-tietokantaan
                await collection.InsertOneAsync(uusiJasen);

                MessageBox.Show("Jäsen tallennettu onnistuneesti!");

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Virhe tallennettaessa: " + ex.Message);
            }
        }

        //Poista tiedot lomakkeelta
        private void ClearForm()
        {
            txbEtunimi.Text = "";
            txbSukunimi.Text = "";
            txbOsoite.Text = "";
            txbPosti.Text = "";
            txbPuhelin.Text = "";
            txbEmail.Text = "";
            datepicker_Aika.SelectedDate = null;
            txb_searchBox.Text = "";
        }

        //Hakee tietoja hakukentän perusteella
        private async Task<List<Jasen>> GetDataFromSearch()
        {
            try
            {
                // haetaan hakusana hakukentästä
                string searchTerm = txb_searchBox.Text;

                if (string.IsNullOrEmpty(searchTerm))
                {
                    MessageBox.Show("Syötä hakusana.");
                    return new List<Jasen>();
                }

                //haetaan tietoja tietokannasta
                var results = await collection.AsQueryable()
                .Where(jasen => jasen.Etunimi.Contains(searchTerm) ||
                            jasen.Sukunimi.Contains(searchTerm) ||
                            jasen.Osoite.Contains(searchTerm) ||
                            jasen.Postinumero.ToString().Contains(searchTerm) ||
                            jasen.Puhelin.ToString().Contains(searchTerm) ||
                            jasen.Sahkoposti.Contains(searchTerm))
                .ToListAsync();

                if (results.Count == 0)
                {
                    MessageBox.Show("Jäsentä ei löytynyt.");
                    return new List<Jasen>();
                }
                return results;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe haettaessa: {ex.Message}");
                return new List<Jasen>();
            }
        }

        //näytetään jäsentiedot
        private string DisplayResults(List<Jasen> results)
        {
            if (results.Count == 0)
            {
                return "Ei tuloksia.";
            }
            else
            {
                var showResult = "";
                foreach (var jasen in results)
                {
                    showResult += $"Etunimi: {jasen.Etunimi}, Sukunimi: {jasen.Sukunimi}, Osoite: {jasen.Osoite}, Postinumero: {jasen.Postinumero}, Puhelin: {jasen.Puhelin}, Sähkoposti: {jasen.Sahkoposti}, JasenydenAlkuPvm: {jasen.JasenydenAlkuPvm} \n";
                }
                return showResult;
            }
        }

        private async void btn_Hae_Click(object sender, RoutedEventArgs e)
        {
            var results = await GetDataFromSearch();

            string displayResult = DisplayResults(results);
            MessageBox.Show(displayResult);

            ClearForm();

        }

        private async void btn_Poista_Click(object sender, RoutedEventArgs e)
        {
            var results = await GetDataFromSearch();

            MessageBoxResult confirmation = MessageBox.Show(
                    $"Löydettiin seuraavat jäsenet. Haluatko varmasti poistaa heidät?\n\n {DisplayResults(results)}",
                    "Vahvista poisto",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (confirmation == MessageBoxResult.Yes)
            {
                foreach (var jasen in results)
                {
                    await collection.DeleteOneAsync(j => j.Id == jasen.Id);
                }

                MessageBox.Show($"{results.Count} jäsentä poistettu.");
            }

            ClearForm();
        }

        private async void btn_Päivitä_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var results = await GetDataFromSearch();

                if (results == null)
                {
                    MessageBox.Show("Hae ensin jäsen ennen päivittämistä.");
                    return;
                }

                //hakee id-arvon ensimmäisestä jäsenestä - results
                var id = results.FirstOrDefault()?.Id;

                //määrittää päivityksen asettamalla Id-kenttä uudelle arvolle - id
                var updateField = Builders<Jasen>.Update.Set(j => j.Id, id);

                //päivitä jatkuvasti vain ei-tyhjät kentät
                if (!string.IsNullOrEmpty(txbEtunimi.Text))
                    updateField = updateField.Set(j => j.Etunimi, txbEtunimi.Text);
                if (!string.IsNullOrEmpty(txbSukunimi.Text))
                    updateField = updateField.Set(j => j.Sukunimi, txbSukunimi.Text);
                if (!string.IsNullOrEmpty(txbOsoite.Text))
                    updateField = updateField.Set(j => j.Osoite, txbOsoite.Text);
                if (!string.IsNullOrEmpty(txbPosti.Text) && int.TryParse(txbPosti.Text, out int postinumero))
                    updateField = updateField.Set(j => j.Postinumero, postinumero);
                if (!string.IsNullOrEmpty(txbPuhelin.Text) && int.TryParse(txbPuhelin.Text, out int puhelin))
                    updateField = updateField.Set(j => j.Puhelin, puhelin);
                if (!string.IsNullOrEmpty(txbEmail.Text))
                    updateField = updateField.Set(j => j.Sahkoposti, txbEmail.Text);
                if (datepicker_Aika.SelectedDate.HasValue)
                    updateField = updateField.Set(j => j.JasenydenAlkuPvm, datepicker_Aika.SelectedDate);

                // suoritetaan päivitys
                var result = await collection.UpdateOneAsync(j => j.Id == id, updateField);
                if (result.ModifiedCount > 0)
                {
                    MessageBox.Show("Jäsen päivitetty onnistuneesti!");
                }
                else
                {
                    MessageBox.Show("Tietoja ei muutettu tai päivitys epäonnistui.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe päivitettäessä: {ex.Message}");
            }

        }
    }
}
