using System;
using System.Data;
using System.Data.SqlClient;

namespace LeaderMain
{
    public partial class LeaderMain : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["Role"].ToString() != "leader")
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
                    LeaderApproveLeave(leaveId, comment);
                }
                else if (action == "reject")
                {
                    LeaderRejectLeave(leaveId, comment);
                }
            }

            // 处理 AJAX 排序请求 - 数据概览学生排名
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

            // 处理 AJAX 请假详情请求
            if (!IsPostBack && Request.QueryString["getDetail"] == "1")
            {
                int leaveId = int.Parse(Request.QueryString["leaveId"]);
                Response.Clear();
                Response.ContentType = "application/json";
                Response.Write(GetLeaderDetailJson(leaveId));
                Response.End();
                return;
            }

            // 处理 AJAX 审批记录请求
            if (!IsPostBack && Request.QueryString["ajaxHistory"] == "1")
            {
                string status = (Request.QueryString["statusFilter"] == "rejected") ? "rejected" : "leader_approved";
                Response.Clear();
                Response.ContentType = "application/json";
                Response.Write(GetLeaderHistoryJson(status));
                Response.End();
                return;
            }

            if (!IsPostBack)
            {
                lblUserName.Text = Session["RealName"] != null ? Session["RealName"].ToString() : Session["Username"].ToString();
                lblTotal.Text = GetTotalLeaves().ToString();
                lblPending.Text = GetLeaderPendingCount().ToString();
                lblApproved.Text = GetApprovedCount().ToString();
                lblRejected.Text = GetRejectedCount().ToString();
                lblLeaderPendingCount.Text = GetLeaderPendingCount().ToString();

                string studentOrderBy = (Request.QueryString["orderBy"] == "total_days") ? "total_days" : "total_count";
                BindClassRanking();
                BindStudentRanking(studentOrderBy);
                BindLeaderPending();
                BindClassStats();
                BindStudentStats(studentOrderBy);

                // 记录当前排序和 section 供前端使用
                ViewState["StudentOrderBy"] = studentOrderBy;
            }
        }

        protected int GetTotalLeaves()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM leave_request WHERE status = 'teacher_approved' OR status = 'leader_approved'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        protected int GetLeaderPendingCount()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM leave_request WHERE status = 'teacher_approved' AND leave_days > 3";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        protected int GetApprovedCount()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM leave_request WHERE status = 'teacher_approved' OR status = 'leader_approved'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        protected int GetRejectedCount()
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

        private void BindStudentRanking(string orderBy = "total_count")
        {
            // orderBy 只能是 total_count 或 total_days
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

        private void BindLeaderPending()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT lr.*, u.real_name as student_name 
                           FROM leave_request lr
                           JOIN [user] u ON lr.student_id = u.user_id
                           WHERE lr.status = 'teacher_approved' AND lr.leave_days > 3
                           ORDER BY lr.create_time DESC";
                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    rptLeaderPending.DataSource = dt;
                    rptLeaderPending.DataBind();
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

        private void BindStudentStats(string orderBy = "total_count")
        {
            // orderBy 只能是 total_count 或 total_days
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

        private void LeaderApproveLeave(int leaveId, string comment)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "UPDATE leave_request SET status = 'leader_approved', approver_id = @approverId, approve_time = GETDATE(), approve_comment = @comment WHERE leave_id = @leaveId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@approverId", Session["UserId"]);
                    cmd.Parameters.AddWithValue("@comment", comment);
                    cmd.Parameters.AddWithValue("@leaveId", leaveId);
                    cmd.ExecuteNonQuery();
                }
            }
            Response.Redirect("LeaderMain.aspx");
        }

        private void LeaderRejectLeave(int leaveId, string comment)
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
            Response.Redirect("LeaderMain.aspx");
        }

        protected double GetPercentage(object total)
        {
            int totalLeaves = GetTotalLeaves();
            if (totalLeaves == 0) return 0;
            return Math.Round((Convert.ToInt32(total) * 100.0) / totalLeaves, 1);
        }

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

        private string GetLeaderHistoryJson(string status)
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

        private string GetLeaderDetailJson(int leaveId)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT lr.*, u.real_name as student_name, u.phone as phone
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
                            string phone = reader["phone"] == DBNull.Value ? "" : reader["phone"].ToString();
                            string reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                            string comment = reader["approve_comment"] == DBNull.Value ? "" : reader["approve_comment"].ToString();
                            return "{\"studentName\":\"" + EscapeJson(reader["student_name"].ToString()) + "\","
                                + "\"phone\":\"" + EscapeJson(phone) + "\","
                                + "\"leaveType\":\"" + EscapeJson(reader["leave_type"].ToString()) + "\","
                                + "\"leaveDays\":" + reader["leave_days"] + ","
                                + "\"startDate\":\"" + reader["start_date"] + "\","
                                + "\"endDate\":\"" + reader["end_date"] + "\","
                                + "\"reason\":\"" + EscapeJson(reason) + "\","
                                + "\"teacherComment\":\"" + EscapeJson(comment) + "\"}";
                        }
                    }
                }
            }
            return "{}";
        }
    }
}