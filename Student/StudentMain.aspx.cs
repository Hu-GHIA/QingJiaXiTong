using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StudentMain
{
    public partial class StudentMain : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("../Default.aspx");
            }

            // 处理 AJAX 取消请假请求
            if (!IsPostBack && Request.QueryString["ajaxCancel"] == "1")
            {
                int leaveId = int.Parse(Request.QueryString["leaveId"]);
                bool success = CancelLeaveAjax(leaveId);
                Response.Clear();
                Response.ContentType = "application/json";
                Response.Write("{\"success\":" + (success ? "true" : "false") + "}");
                Response.End();
                return;
            }

            if (!IsPostBack)
            {
                // 显示欢迎用户名
                lblUserName.Text = Session["RealName"] != null ? Session["RealName"].ToString() : Session["Username"].ToString();

                // 处理提交成功后的提示
                if (Request.QueryString["msg"] == "success")
                {
                    litMessage.Text = "<div class='alert alert-success'>请假申请提交成功！</div>";
                }
                else if (Request.QueryString["msg"] == "cancelled")
                {
                    litMessage.Text = "<div class='alert alert-info'>请假申请已取消。</div>";
                }

                // 预填充联系电话
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT phone FROM [user] WHERE user_id = @userId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", Session["UserId"]);
                        object phone = cmd.ExecuteScalar();
                        if (phone != DBNull.Value)
                        {
                            txtPhone.Text = phone.ToString();
                        }
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string leaveType = ddlLeaveType.SelectedValue;
            string phone = txtPhone.Text.Trim();
            string startDate = txtStartDate.Text;
            string endDate = txtEndDate.Text;
            string reason = txtReason.Text.Trim();

            if (string.IsNullOrEmpty(leaveType) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(reason))
            {
                litMessage.Text = "<div class='alert alert-danger'>请填写完整信息</div>";
                return;
            }

            DateTime start = DateTime.Parse(startDate);
            DateTime end = DateTime.Parse(endDate);
            int leaveDays = (int)(end - start).TotalDays + 1;

            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "INSERT INTO leave_request (student_id, class_id, dept_id, leave_type, start_date, end_date, leave_days, reason, status) VALUES (@studentId, @classId, @deptId, @leaveType, @startDate, @endDate, @leaveDays, @reason, 'pending')";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", Session["UserId"]);
                    cmd.Parameters.AddWithValue("@classId", Session["ClassId"] ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@deptId", Session["DeptId"] ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@leaveType", leaveType);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@leaveDays", leaveDays);
                    cmd.Parameters.AddWithValue("@reason", reason);

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        Response.Redirect("StudentMain.aspx?msg=success", false);
                    }
                    else
                    {
                        litMessage.Text = "<div class='alert alert-danger'>提交失败，请重试</div>";
                    }
                }
            }
        }

        protected string GetStatusClass(string status)
        {
            switch (status)
            {
                case "pending":
                    return "bg-warning";
                case "teacher_approved":
                case "leader_approved":
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
                case "pending":
                    return "待审批";
                case "teacher_approved":
                    return "已通过(班主任)";
                case "leader_approved":
                    return "已通过(领导)";
                case "rejected":
                    return "已拒绝";
                default:
                    return status;
            }
        }

        protected void gvLeaveRecords_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindLeaveRecords();
            }
        }

        private void BindLeaveRecords()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM leave_request WHERE student_id = @studentId ORDER BY create_time DESC";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", Session["UserId"]);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvLeaveRecords.DataSource = dt;
                        gvLeaveRecords.DataBind();
                    }
                }
            }
        }

        protected void gvLeaveRecords_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // 已改用 AJAX 方式处理取消，此方法保留备用
        }

        protected void gvLeaveRecords_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                object leaveId = DataBinder.Eval(e.Row.DataItem, "leave_id");
                if (leaveId != null)
                {
                    e.Row.Attributes["data-leave-id"] = leaveId.ToString();
                }
            }
        }

        private bool CancelLeaveAjax(int leaveId)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveSystemConnectionString"].ConnectionString))
            {
                conn.Open();
                string sql = "DELETE FROM leave_request WHERE leave_id = @leaveId AND student_id = @studentId AND status = 'pending'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@leaveId", leaveId);
                    cmd.Parameters.AddWithValue("@studentId", Session["UserId"]);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}