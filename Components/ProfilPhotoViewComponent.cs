using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using KonserBiletim.ViewModels;

namespace KonserBiletim.ViewComponents
{
    public class ProfilPhotoViewComponent : ViewComponent
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new ProfilViewModel();
            string user_id = HttpContext.Session.GetString("UserID");
            string userRole = HttpContext.Session.GetString("UserRole");

            if (!string.IsNullOrEmpty(user_id) && !string.IsNullOrEmpty(userRole))
            {
                int userID;
                if (Int32.TryParse(user_id, out userID))
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        await con.OpenAsync();
                        string query = "";

                        if (userRole == "Musteri")
                        {
                            query = "SELECT p.profil_foto_path FROM Musteri m LEFT JOIN Profil p ON m.musteri_id = p.userID WHERE m.musteri_id = @UserID";
                        }
                        else if (userRole == "Organizator")
                        {
                            query = "SELECT p.profil_foto_path FROM Organizator o LEFT JOIN ProfilOrg p ON o.orgID = p.userID WHERE o.orgID = @UserID";
                        }

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                            using (SqlDataReader dreader = await cmd.ExecuteReaderAsync())
                            {
                                if (await dreader.ReadAsync())
                                {
                                    model.UserID = userID;
                                    model.ProfilFotoPath = dreader["profil_foto_path"]?.ToString();
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.ProfilFotoPath))
                {
                    model.ProfilFotoPath = $"~/uploads/{model.ProfilFotoPath}";
                }
            }

            return View(model);
        }
    }
}
