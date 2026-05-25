using System;
using System.Data;
using System.Data.SqlClient;
//using System.Web.Security;
using System.Web.UI.WebControls;

namespace Default
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Cookies["UserInfo"] != null)
            {
                Response.Redirect("Student/StudentMain.aspx");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                litError.Text = "请填写用户名和密码";
                return;
            }

            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM [user] WHERE username = @username AND password = @password";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string role = reader["role"].ToString();

                            // 登录成功，保存用户信息到Session
                            Session["UserId"] = reader["user_id"];
                            Session["Username"] = reader["username"];
                            Session["RealName"] = reader["real_name"];
                            Session["Role"] = role;
                            Session["ClassId"] = reader["class_id"];
                            Session["DeptId"] = reader["dept_id"];

                            // 根据账号角色自动跳转到对应页面
                            switch (role)
                            {
                                case "student":
                                    Response.Redirect("Student/StudentMain.aspx");
                                    break;
                                case "teacher":
                                    Response.Redirect("Teacher/TeacherMain.aspx");
                                    break;
                                case "leader":
                                    Response.Redirect("Leader/LeaderMain.aspx");
                                    break;
                                default:
                                    litError.Text = "账号角色未知，请联系管理员";
                                    break;
                            }
                        }
                        else
                        {
                            litError.Text = "用户名或密码不正确";
                        }
                    }
                }
            }
        }
    }
}