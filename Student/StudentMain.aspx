<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StudentMain.aspx.cs" Inherits="StudentMain.StudentMain" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>学生请假 - 高校请假系统</title>
    <link href="https://cdn.bootcdn.net/ajax/libs/bootstrap/5.1.3/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.bootcdn.net/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    <style>
        .sidebar {
            background: linear-gradient(180deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            color: white;
        }
        .nav-link {
            color: rgba(255,255,255,0.8);
            border-radius: 10px;
            margin: 5px 0;
        }
        .nav-link:hover, .nav-link.active {
            background: rgba(255,255,255,0.2);
            color: white;
        }
        .card {
            border: none;
            border-radius: 15px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.08);
        }
        .status-pending { color: #ffc107; }
        .status-approved { color: #28a745; }
        .status-rejected { color: #dc3545; }
        .badge-status {
            font-size: 0.85rem;
            padding: 6px 12px;
            border-radius: 20px;
        }
        .bg-warning { background-color: #ffc107 !important; color: #000 !important; }
        .bg-success { background-color: #28a745 !important; color: #fff !important; }
        .bg-danger { background-color: #dc3545 !important; color: #fff !important; }
        @media (max-width: 768px) {
            .sidebar {
                min-height: auto;
            }
        }
        @keyframes fadeInRight {
            from { opacity: 0; transform: translateX(50px); }
            to { opacity: 1; transform: translateX(0); }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container-fluid">
            <div class="row">
                <nav class="col-md-2 sidebar py-4" id="sidebar">
                    <div class="text-center mb-4">
                        <i class="fas fa-calendar-check fa-2x mb-2"></i>
                        <h5>请假系统</h5>
                        <small>学生端</small>
                    </div>
                    <ul class="nav flex-column px-3">
                        <li class="nav-item">
                            <a class="nav-link active" href="#" onclick="showSection('apply')">
                                <i class="fas fa-plus-circle me-2"></i>提交请假
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="#" onclick="showSection('records')">
                                <i class="fas fa-history me-2"></i>请假记录
                            </a>
                        </li>
                        <li class="nav-item mt-5">
                            <a class="nav-link" href="../Default.aspx">
                                <i class="fas fa-sign-out-alt me-2"></i>退出登录
                            </a>
                        </li>
                    </ul>
                </nav>

                <main class="col-md-10 ms-sm-auto px-4 py-4">
                    <div class="d-flex justify-content-between align-items-center mb-4">
                        <h4><i class="fas fa-user-graduate me-2"></i>欢迎，<span id="userName"><asp:Label ID="lblUserName" runat="server"></asp:Label></span></h4>
                        <button class="btn btn-outline-primary d-md-none" onclick="toggleSidebar()">
                            <i class="fas fa-bars"></i>
                        </button>
                    </div>

                    <div id="applySection">
                        <div class="card">
                            <div class="card-header bg-white py-3">
                                <h5 class="mb-0"><i class="fas fa-paper-plane me-2 text-primary"></i>提交请假申请</h5>
                            </div>
                            <div class="card-body">
                                <div class="row g-3">
                                    <div class="col-md-6">
                                        <label class="form-label">请假类型</label>
                                        <asp:DropDownList ID="ddlLeaveType" runat="server" class="form-select" required>
                                            <asp:ListItem Value="">请选择类型</asp:ListItem>
                                            <asp:ListItem Value="事假">事假</asp:ListItem>
                                            <asp:ListItem Value="病假">病假</asp:ListItem>
                                            <asp:ListItem Value="其他">其他</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">联系电话</label>
                                        <asp:TextBox ID="txtPhone" runat="server" class="form-control" placeholder="请输入手机号" required></asp:TextBox>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">开始日期</label>
                                        <asp:TextBox ID="txtStartDate" runat="server" class="form-control" type="date" required></asp:TextBox>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">结束日期</label>
                                        <asp:TextBox ID="txtEndDate" runat="server" class="form-control" type="date" required></asp:TextBox>
                                    </div>
                                    <div class="col-12">
                                        <label class="form-label">请假天数</label>
                                        <asp:TextBox ID="txtLeaveDays" runat="server" class="form-control" ReadOnly="true" placeholder="系统自动计算"></asp:TextBox>
                                        <small class="text-muted">系统将根据日期自动计算</small>
                                    </div>
                                    <div class="col-12">
                                        <label class="form-label">请假原因</label>
                                        <asp:TextBox ID="txtReason" runat="server" class="form-control" TextMode="MultiLine" Rows="4" placeholder="请详细描述请假原因..." required></asp:TextBox>
                                    </div>
                                    <div class="col-12">
                                        <div class="alert alert-info mb-0">
                                            <i class="fas fa-info-circle me-2"></i>
                                            <strong>审批说明：</strong>
                                            <ul class="mb-0 mt-2">
                                                <li>请假 ≤ 3天：由班主任/辅导员审批</li>
                                                <li>请假 > 3天：需班主任初审后，再由学院领导审批</li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>
                                <asp:Button ID="btnSubmit" runat="server" class="btn btn-primary mt-4 px-5" Text="提交申请" OnClick="btnSubmit_Click" />
                                <div class="d-block mt-3"><asp:Literal ID="litMessage" runat="server"></asp:Literal></div>
                            </div>
                        </div>
                    </div>

                    <div id="recordsSection" style="display:none;">
                        <div class="card">
                            <div class="card-header bg-white py-3">
                                <h5 class="mb-0"><i class="fas fa-list-alt me-2 text-primary"></i>我的请假记录</h5>
                            </div>
                            <div class="card-body">
                                <div class="table-responsive">
                                    <asp:GridView ID="gvLeaveRecords" runat="server" class="table table-hover" AutoGenerateColumns="false" OnLoad="gvLeaveRecords_Load" OnRowDataBound="gvLeaveRecords_RowDataBound">
                                        <Columns>
                                            <asp:BoundField DataField="leave_type" HeaderText="请假类型" />
                                            <asp:BoundField DataField="start_date" HeaderText="开始日期" />
                                            <asp:BoundField DataField="end_date" HeaderText="结束日期" />
                                            <asp:BoundField DataField="leave_days" HeaderText="天数" />
                                            <asp:BoundField DataField="reason" HeaderText="原因" />
                                            <asp:TemplateField HeaderText="状态">
                                                <ItemTemplate>
                                                    <span class='badge badge-status <%# GetStatusClass(Eval("status").ToString()) %>'>
                                                        <%# GetStatusText(Eval("status").ToString()) %>
                                                    </span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="approve_comment" HeaderText="审批意见" />
                                            <asp:BoundField DataField="create_time" HeaderText="申请时间" />
                                            <asp:TemplateField HeaderText="操作">
                                                <ItemTemplate>
                                                    <%# Eval("status").ToString() == "pending" ?
                                                        "<button type=\"button\" class=\"btn btn-outline-danger btn-sm\" onclick=\"cancelLeave(" + Eval("leave_id") + ", this)\">取消请假</button>" : "" %>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </div>
                </main>
            </div>
        </div>
    </form>
    <script src="https://cdn.bootcdn.net/ajax/libs/bootstrap/5.1.3/js/bootstrap.bundle.min.js"></script>
    <script>
        function showSection(section) {
            document.getElementById('applySection').style.display = section === 'apply' ? 'block' : 'none';
            document.getElementById('recordsSection').style.display = section === 'records' ? 'block' : 'none';

            document.querySelectorAll('.nav-link').forEach(link => link.classList.remove('active'));
            event.target.classList.add('active');
        }

        // AJAX 无刷新取消请假
        function cancelLeave(leaveId, btn) {
            if (!confirm('确定要取消这条请假申请吗？取消后无法恢复。')) return;
            btn.disabled = true;
            btn.textContent = '取消中...';
            fetch('StudentMain.aspx?ajaxCancel=1&leaveId=' + leaveId)
                .then(function(r) { return r.json(); })
                .then(function(data) {
                    if (data.success) {
                        var row = btn.closest('tr');
                        row.style.transition = 'opacity 0.3s';
                        row.style.opacity = '0';
                        setTimeout(function() { row.remove(); }, 300);
                        showToast('请假申请已取消', 'success');
                    } else {
                        showToast('取消失败，可能已不在待审批状态', 'danger');
                        btn.disabled = false;
                        btn.textContent = '取消请假';
                    }
                })
                .catch(function() {
                    showToast('网络错误，请重试', 'danger');
                    btn.disabled = false;
                    btn.textContent = '取消请假';
                });
        }

        function showToast(message, type) {
            var toast = document.createElement('div');
            toast.className = 'alert alert-' + type + ' position-fixed';
            toast.style.cssText = 'top:20px;right:20px;z-index:9999;min-width:250px;animation:fadeInRight 0.3s ease;';
            toast.textContent = message;
            document.body.appendChild(toast);
            setTimeout(function() {
                toast.style.transition = 'opacity 0.3s';
                toast.style.opacity = '0';
                setTimeout(function() { toast.remove(); }, 300);
            }, 2500);
        }

        function toggleSidebar() {
            document.getElementById('sidebar').classList.toggle('show');
        }

        document.getElementById('<%= txtStartDate.ClientID %>').addEventListener('change', calculateDays);
        document.getElementById('<%= txtEndDate.ClientID %>').addEventListener('change', calculateDays);

        function calculateDays() {
            const start = document.getElementById('<%= txtStartDate.ClientID %>').value;
            const end = document.getElementById('<%= txtEndDate.ClientID %>').value;
            
            if (start && end) {
                const startDate = new Date(start);
                const endDate = new Date(end);
                const diffTime = Math.abs(endDate - startDate);
                const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
                document.getElementById('<%= txtLeaveDays.ClientID %>').value = diffDays + ' 天';
                
                if (diffDays > 3) {
                    document.getElementById('<%= txtLeaveDays.ClientID %>').style.color = '#dc3545';
                } else {
                    document.getElementById('<%= txtLeaveDays.ClientID %>').style.color = '#28a745';
                }
            }
        }

        const today = new Date().toISOString().split('T')[0];
        document.getElementById('<%= txtStartDate.ClientID %>').min = today;
        document.getElementById('<%= txtEndDate.ClientID %>').min = today;
    </script>
</body>
</html>