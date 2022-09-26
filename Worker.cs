using System.Xml;
using Microsoft.Data.SqlClient;

namespace CentralBankCurrencyService
{
    public class Worker : BackgroundService
    {
        public static SqlConnection baglanti = new SqlConnection(@"server=.; Initial Catalog=DBHondaDoviz;Integrated Security=SSPI");

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime tarih = DateTime.Now.Date;

                if (DateTime.Now.Hour >= 16)
                {
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    {
                        tarih = DateTime.Now.AddDays(-1);
                    }
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        tarih = DateTime.Now.AddDays(-2);
                    }

                    baglanti.Close();
                    baglanti.Open();

                    SqlCommand sqlCommand = new SqlCommand("select count(kurId) from TBLHondaDoviz where tarih=@tarih", baglanti);
                    sqlCommand.Parameters.AddWithValue("@tarih", tarih);

                    if (Convert.ToInt64(sqlCommand.ExecuteScalar()) <= 0)
                    {
                        string bugun = "https://www.tcmb.gov.tr/kurlar/today.xml";
                        var xmldoc = new XmlDocument();
                        xmldoc.Load(bugun);


                        string USD = xmldoc.SelectSingleNode("Tarih_Date/Currency [@Kod='USD']/BanknoteSelling").InnerXml;

                        string EUR = xmldoc.SelectSingleNode("Tarih_Date/Currency [@Kod='EUR']/BanknoteSelling").InnerXml;

                        string GBP = xmldoc.SelectSingleNode("Tarih_Date/Currency [@Kod='GBP']/BanknoteSelling").InnerXml;

                        string JPY = xmldoc.SelectSingleNode("Tarih_Date/Currency [@Kod='JPY']/BanknoteSelling").InnerXml;
                        Double JPY_Convert = Convert.ToDouble(JPY.Replace(".", ","));
                        JPY_Convert = JPY_Convert / 100;
                        JPY = Convert.ToString(JPY_Convert);

                        SqlCommand HondaCurrency = new SqlCommand("insert into TBLHondaDoviz (dovizTuru,deger,tarih) values (@dovizTuru,@deger,@tarih)", baglanti);

                        HondaCurrency.Parameters.AddWithValue("@dovizTuru", "USD");
                        HondaCurrency.Parameters.AddWithValue("@deger", USD);
                        HondaCurrency.Parameters.AddWithValue("@tarih", tarih);
                        HondaCurrency.ExecuteNonQuery();

                        HondaCurrency.Parameters["@dovizTuru"].Value = "EUR";
                        HondaCurrency.Parameters["@deger"].Value = EUR;
                        HondaCurrency.Parameters["@tarih"].Value = tarih;
                        HondaCurrency.ExecuteNonQuery();

                        HondaCurrency.Parameters["@dovizTuru"].Value = "GBP";
                        HondaCurrency.Parameters["@deger"].Value = GBP;
                        HondaCurrency.Parameters["@tarih"].Value = tarih;
                        HondaCurrency.ExecuteNonQuery();

                        HondaCurrency.Parameters["@dovizTuru"].Value = "JPY";
                        HondaCurrency.Parameters["@deger"].Value = JPY;
                        HondaCurrency.Parameters["@tarih"].Value = tarih;
                        HondaCurrency.ExecuteNonQuery();
                    }
                }
                await Task.Delay(1000 * 60 * 10, stoppingToken);
            }
        }
    }
}