using KonserBiletim.Models;
using KonserBiletim.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;
using Dapper;

namespace KonserBiletim.Controllers
{
    public class AdminController : Controller
    {
        private readonly string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        public IActionResult Index()
        {
            IEnumerable<BiletimViewModel> biletSatisVerileri;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = "SELECT satinAlmaTarihi AS Tarih, COUNT(*) AS BiletSayisi FROM Biletim GROUP BY satinAlmaTarihi ORDER BY Tarih";

                biletSatisVerileri = con.Query<BiletimViewModel>(query);

            }

            var model = new MasterViewModel
            {
                BiletSatisVerileri = biletSatisVerileri
            };

            return View(model);
        }

        public IActionResult AdminOnay()
        {
            var model = new MasterViewModel
            {
                Admin = new AdminViewModel
                {
                    pendingOrganizators = new List<Organizator>()
                }
            };

            string query = "SELECT orgID, orgMail, orgName, role, IsApproved, orgSurname, IsRejected FROM Organizator WHERE IsApproved = 0 AND IsRejected = 0";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var organizator = new Organizator
                            {
                                orgID = reader.IsDBNull(reader.GetOrdinal("orgID")) ? 0 : reader.GetInt32(reader.GetOrdinal("orgID")),
                                orgMail = reader.IsDBNull(reader.GetOrdinal("orgMail")) ? string.Empty : reader.GetString(reader.GetOrdinal("orgMail")),
                                orgName = reader.IsDBNull(reader.GetOrdinal("orgName")) ? string.Empty : reader.GetString(reader.GetOrdinal("orgName")),
                                role = reader.IsDBNull(reader.GetOrdinal("role")) ? string.Empty : reader.GetString(reader.GetOrdinal("role")),
                                IsApproved = reader.IsDBNull(reader.GetOrdinal("IsApproved")) ? false : reader.GetBoolean(reader.GetOrdinal("IsApproved")),
                                orgSurname = reader.IsDBNull(reader.GetOrdinal("orgSurname")) ? string.Empty : reader.GetString(reader.GetOrdinal("orgSurname")),
                                IsRejected = reader.IsDBNull(reader.GetOrdinal("IsRejected")) ? false : reader.GetBoolean(reader.GetOrdinal("IsRejected"))
                            };

                            model.Admin.pendingOrganizators.Add(organizator);
                        }
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult AdminOnay(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Geçersiz ID");
            }

            string query = "UPDATE Organizator SET IsApproved = 1, IsRejected = 0 WHERE orgID = @orgID";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@orgID", id);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["mesaj"] = "Organizatör onaylandı";
            return RedirectToAction("AdminOnay");
        }

        [HttpPost]
        public async Task<IActionResult> AdminRet(int id)
        {
            string query = "UPDATE Organizator SET IsApproved = 0, IsRejected = 1 WHERE orgID = @orgID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@orgID", id);
                    conn.Open();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            TempData["mesaj"] = "Organizatör reddedildi.";
            return RedirectToAction("AdminOnay");
        }
    }
}
