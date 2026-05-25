using System;
using System.Data;
using System.Data.SqlClient;

namespace TeacherMain
{
    public partial class TeacherMain : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["Role"].ToString() != "teacher")
            {
                Response.Redirect("../Default.aspx");
            }

            // 处理审批操作
            if (!IsPostBack && Request.QueryString["action"] != null)
            {
                string action = Request.QueryString["action"];
                int leaveId = int.Parse(Request.QueryString["leaveId"]);
                string comment = Request.QueryString["comment"];

                if (action == "approve")
                {
                    ApproveLeave(leaveId, comment);
                }
                else if (action == "reject")
                {
                    RejectLeave(leaveId, comment);
                }
            }

            // 处理 AJAX 详情请求
            if (!IsPostBack && Request.QueryString["getDetail"] == "1")
            {
                int detailId = int.Parse(Request.QueryString["leaveId"]);
                Response.Clear();
                Response.ContentType = "application/json";
                Response.Write(GetLeaveDetailJson(detailId));
                Response.End();
                return;
            }

            // 处理 AJAX 排序请求 - 学生排名
            if (!IsPostBack && Request.QueryString["ajaxSort"] == "ranking")
            {
                string orderBy = (Request.QueryString["orderBy"] == "total_days") ? "total_days" : "total_count";
                Response.Clear();
                Response.ContentType = "application/json";
                Response.Write(GetStudentRankingJson(orderBy));
                Response.End();
                return;
            }

            // 处理 AJAX 排序请求 - 学生统计
            if (!IsPostBack && Request.QueryString["ajaxSort"] == "studentStats")
            {
                string orderBy = (Request.QueryString["orderBy"] == "total_days") ? "total_days" : "total_count";
                Response.Clear();
                Response.ContentType = "application/json";
                Response.Write(GetStudentStatsJson(orderBy));
                Response.End();
                return;
            }

            // 处理 AJAX 审批记录请求
            if (!IsPostBack && Request.QueryString["ajaxHistory"] == "1")
            {
                string status = (Request.QueryString["statusFilter"] == "rejected") ? "rejected" : "teacher_approved";
                Response.Clear();
                Response.ContentType = "application/json";
                Response.Write(GetTeacherHistoryJson(status));
                Response.End();
                return;
            }

            if (!IsPostBack)
            {
                lblUserName.Text = Session["RealName"] != null ? Session["RealName"].ToString() : Session["Username"].ToString();
                lblPendingCount.Text = GetPendingCount().ToString();

                BindPendingLeaves();
                BindClassRanking();
                BindStudentRanking("total_count");
                BindClassStats();
                BindStudentStats("total_count");

                lblTotalLeaves.Text = GetTeacherTotalLeaves().ToString();
                lblTeacherApproved.Text = GetTeacherApprovedCount().ToString();
                lblTeacherRejected.Text = GetTeacherRejectedCount().ToString();
            }
        }

        protected int GetPendingCount()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM leave_request WHERE status = 'pending'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private void BindPendingLeaves()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT lr.*, u.real_name as student_name 
                           FROM leave_request lr
                           JOIN [user] u ON lr.student_id = u.user_id
                           WHERE lr.status = 'pending' ORDER BY lr.create_time DESC";
                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    rptPendingLeaves.DataSource = dt;
                    rptPendingLeaves.DataBind();
                }
            }
        }

        private void ApproveLeave(int leaveId, string comment)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "UPDATE leave_request SET status = 'teacher_approved', approver_id = @approverId, approve_time = GETDATE(), approve_comment = @comment WHERE leave_id = @leaveId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@approverId", Session["UserId"]);
                    cmd.Parameters.AddWithValue("@comment", comment);
                    cmd.Parameters.AddWithValue("@leaveId", leaveId);
                    cmd.ExecuteNonQuery();
                }
            }
            Response.Redirect("TeacherMain.aspx");
        }

        private void RejectLeave(int leaveId, string comment)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "UPDATE leave_request SET status = 'rejected', approver_id = @approverId, approve_time = GETDATE(), approve_comment = @comment WHERE leave_id = @leaveId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@approverId", Session["UserId"]);
                    cmd.Parameters.AddWithValue("@comment", comment);
                    cmd.Parameters.AddWithValue("@leaveId", leaveId);
                    cmd.ExecuteNonQuery();
                }
            }
            Response.Redirect("TeacherMain.aspx");
        }

        protected string GetStatusClass(string status)
        {
            switch (status)
            {
                case "teacher_approved":
                    return "bg-success";
                case "rejected":
                    return "bg-danger";
                default:
                    return "bg-secondary";
            }
        }

        protected string GetStatusText(string status)
        {
            switch (status)
            {
                case "teacher_approved":
                    return "已批准";
                case "rejected":
                    return "已拒绝";
                default:
                    return status;
            }
        }

        private string GetLeaveDetailJson(int leaveId)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT lr.*, u.real_name as student_name, u.phone 
                               FROM leave_request lr
                               JOIN [user] u ON lr.student_id = u.user_id
                               WHERE lr.leave_id = @leaveId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@leaveId", leaveId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string studentName = reader["student_name"].ToString();
                            string phone = reader["phone"].ToString();
                            string leaveType = reader["leave_type"].ToString();
                            int leaveDays = Convert.ToInt32(reader["leave_days"]);
                            string startDate = Convert.ToDateTime(reader["start_date"]).ToString("yyyy-MM-dd");
                            string endDate = Convert.ToDateTime(reader["end_date"]).ToString("yyyy-MM-dd");
                            string reason = reader["reason"].ToString();
                            string approveComment = reader["approve_comment"] == DBNull.Value ? "" : reader["approve_comment"].ToString();
                            string createTime = reader["create_time"].ToString();

                            return "{\"studentName\":\"" + studentName.Replace("\"", "\\\"") + "\","
                                 + "\"phone\":\"" + phone + "\","
                                 + "\"leaveType\":\"" + leaveType + "\","
                                 + "\"leaveDays\":\"" + leaveDays + "\","
                                 + "\"startDate\":\"" + startDate + "\","
                                 + "\"endDate\":\"" + endDate + "\","
                                 + "\"reason\":\"" + reason.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "<br/>") + "\","
                                 + "\"approveComment\":\"" + approveComment.Replace("\"", "\\\"") + "\","
                                 + "\"createTime\":\"" + createTime + "\"}";
                        }
                    }
                }
            }
            return "{}";
        }

        #region 统计功能

        protected int GetTeacherTotalLeaves()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM leave_request WHERE status IN ('teacher_approved','leader_approved')";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        protected int GetTeacherApprovedCount()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM leave_request WHERE status IN ('teacher_approved','leader_approved')";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        protected int GetTeacherRejectedCount()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM leave_request WHERE status = 'rejected'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private void BindClassRanking()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT TOP 6 c.class_name, 
                           COUNT(*) as total, 
                           SUM(CASE WHEN lr.leave_type = '事假' THEN 1 ELSE 0 END) as 事假, 
                           SUM(CASE WHEN lr.leave_type = '病假' THEN 1 ELSE 0 END) as 病假, 
                           SUM(CASE WHEN lr.leave_type = '其他' THEN 1 ELSE 0 END) as 其他 
                           FROM leave_request lr
                           JOIN class c ON lr.class_id = c.class_id
                           WHERE lr.status IN ('teacher_approved','leader_approved')
                           GROUP BY c.class_name
                           ORDER BY total DESC";
                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    rptClassRanking.DataSource = dt;
                    rptClassRanking.DataBind();
                }
            }
        }

        private void BindStudentRanking(string orderBy)
        {
            string orderColumn = (orderBy == "total_days") ? "total_days" : "total_count";
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT TOP 6 u.real_name as student_name, 
                           c.class_name, 
                           COUNT(*) as total_count, 
                           SUM(lr.leave_days) as total_days 
                           FROM leave_request lr
                           JOIN [user] u ON lr.student_id = u.user_id
                           JOIN class c ON u.class_id = c.class_id
                           WHERE lr.status IN ('teacher_approved','leader_approved')
                           GROUP BY u.real_name, c.class_name
                           ORDER BY " + orderColumn + " DESC";
                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    rptStudentRanking.DataSource = dt;
                    rptStudentRanking.DataBind();
                }
            }
        }

        private void BindClassStats()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT c.class_name, 
                           COUNT(*) as total, 
                           SUM(CASE WHEN lr.leave_type = '事假' THEN 1 ELSE 0 END) as 事假, 
                           SUM(CASE WHEN lr.leave_type = '病假' THEN 1 ELSE 0 END) as 病假, 
                           SUM(CASE WHEN lr.leave_type = '其他' THEN 1 ELSE 0 END) as 其他 
                           FROM leave_request lr
                           JOIN class c ON lr.class_id = c.class_id
                           WHERE lr.status IN ('teacher_approved','leader_approved')
                           GROUP BY c.class_name
                           ORDER BY total DESC";
                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    gvClassStats.DataSource = dt;
                    gvClassStats.DataBind();
                }
            }
        }

        private void BindStudentStats(string orderBy)
        {
            string orderColumn = (orderBy == "total_days") ? "total_days" : "total_count";
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT u.real_name as student_name, 
                           c.class_name, 
                           COUNT(*) as total_count, 
                           SUM(lr.leave_days) as total_days, 
                           SUM(CASE WHEN lr.leave_type = '事假' THEN 1 ELSE 0 END) as 事假, 
                           SUM(CASE WHEN lr.leave_type = '病假' THEN 1 ELSE 0 END) as 病假 
                           FROM leave_request lr
                           JOIN [user] u ON lr.student_id = u.user_id
                           JOIN class c ON u.class_id = c.class_id
                           WHERE lr.status IN ('teacher_approved','leader_approved')
                           GROUP BY u.real_name, c.class_name
                           ORDER BY " + orderColumn + " DESC";
                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    gvStudentStats.DataSource = dt;
                    gvStudentStats.DataBind();
                }
            }
        }

        #endregion

        #region JSON 方法

        private string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
        }

        private string GetStudentRankingJson(string orderBy)
        {
            string orderColumn = (orderBy == "total_days") ? "total_days" : "total_count";
            System.Text.StringBuilder json = new System.Text.StringBuilder();
            json.Append("[");
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT TOP 6 u.real_name as student_name, 
                           c.class_name, 
                           COUNT(*) as total_count, 
                           SUM(lr.leave_days) as total_days 
                           FROM leave_request lr
                           JOIN [user] u ON lr.student_id = u.user_id
                           JOIN class c ON u.class_id = c.class_id
                           WHERE lr.status IN ('teacher_approved','leader_approved')
                           GROUP BY u.real_name, c.class_name
                           ORDER BY " + orderColumn + " DESC";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    bool first = true;
                    while (reader.Read())
                    {
                        if (!first) json.Append(",");
                        first = false;
                        json.Append("{\"student_name\":\"" + EscapeJson(reader["student_name"].ToString()) + "\","
                            + "\"class_name\":\"" + EscapeJson(reader["class_name"].ToString()) + "\","
                            + "\"total_count\":" + reader["total_count"] + ","
                            + "\"total_days\":" + reader["total_days"] + "}");
                    }
                }
            }
            json.Append("]");
            return json.ToString();
        }

        private string GetStudentStatsJson(string orderBy)
        {
            string orderColumn = (orderBy == "total_days") ? "total_days" : "total_count";
            System.Text.StringBuilder json = new System.Text.StringBuilder();
            json.Append("[");
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT u.real_name as student_name, 
                           c.class_name, 
                           COUNT(*) as total_count, 
                           SUM(lr.leave_days) as total_days, 
                           SUM(CASE WHEN lr.leave_type = '事假' THEN 1 ELSE 0 END) as 事假, 
                           SUM(CASE WHEN lr.leave_type = '病假' THEN 1 ELSE 0 END) as 病假 
                           FROM leave_request lr
                           JOIN [user] u ON lr.student_id = u.user_id
                           JOIN class c ON u.class_id = c.class_id
                           WHERE lr.status IN ('teacher_approved','leader_approved')
                           GROUP BY u.real_name, c.class_name
                           ORDER BY " + orderColumn + " DESC";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    bool first = true;
                    while (reader.Read())
                    {
                        if (!first) json.Append(",");
                        first = false;
                        json.Append("{\"student_name\":\"" + EscapeJson(reader["student_name"].ToString()) + "\","
                            + "\"class_name\":\"" + EscapeJson(reader["class_name"].ToString()) + "\","
                            + "\"total_count\":" + reader["total_count"] + ","
                            + "\"total_days\":" + reader["total_days"] + ","
                            + "\"shijia\":" + reader["事假"] + ","
                            + "\"bingjia\":" + reader["病假"] + "}");
                    }
                }
            }
            json.Append("]");
            return json.ToString();
        }

        private string GetTeacherHistoryJson(string status)
        {
            System.Text.StringBuilder json = new System.Text.StringBuilder();
            json.Append("[");

            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT lr.leave_id, u.real_name as student_name, c.class_name,
                               lr.leave_type, lr.start_date, lr.end_date, lr.leave_days, lr.reason,
                               lr.approve_comment, lr.approve_time
                               FROM leave_request lr
                               JOIN [user] u ON lr.student_id = u.user_id
                               JOIN class c ON u.class_id = c.class_id
                               WHERE lr.status = @status AND lr.approver_id = @approverId
                               ORDER BY lr.approve_time DESC";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@approverId", Session["UserId"]);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool first = true;
                        while (reader.Read())
                        {
                            if (!first) json.Append(",");
                            first = false;
                            string comment = reader["approve_comment"] == DBNull.Value ? "" : reader["approve_comment"].ToString();
                            string approveTime = reader["approve_time"] == DBNull.Value ? "" : Convert.ToDateTime(reader["approve_time"]).ToString("yyyy-MM-dd HH:mm");
                            json.Append("{\"leave_id\":" + reader["leave_id"]
                                + ",\"student_name\":\"" + EscapeJson(reader["student_name"].ToString()) + "\""
                                + ",\"class_name\":\"" + EscapeJson(reader["class_name"].ToString()) + "\""
                                + ",\"leave_type\":\"" + EscapeJson(reader["leave_type"].ToString()) + "\""
                                + ",\"start_date\":\"" + reader["start_date"] + "\""
                                + ",\"end_date\":\"" + reader["end_date"] + "\""
                                + ",\"leave_days\":" + reader["leave_days"]
                                + ",\"reason\":\"" + EscapeJson(reader["reason"].ToString()) + "\""
                                + ",\"approve_comment\":\"" + EscapeJson(comment) + "\""
                                + ",\"approve_time\":\"" + approveTime + "\"}");
                        }
                    }
                }
            }

            json.Append("]");
            return json.ToString();
        }

        #endregion
    }
}