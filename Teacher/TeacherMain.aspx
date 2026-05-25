<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TeacherMain.aspx.cs" Inherits="TeacherMain.TeacherMain" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>班主任审批 - 高校请假系统</title>
    <link href="https://cdn.bootcdn.net/ajax/libs/bootstrap/5.1.3/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.bootcdn.net/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    <style>
        .sidebar {
            background: linear-gradient(180deg, #28a745 0%, #20c997 100%);
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
        .badge-status {
            font-size: 0.85rem;
            padding: 6px 12px;
            border-radius: 20px;
        }
        .approval-card {
            border-left: 4px solid #28a745;
            transition: all 0.3s ease;
        }
        .approval-card:hover {
            transform: translateX(5px);
            box-shadow: 0 5px 20px rgba(0,0,0,0.15);
        }
        .detail-row {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 10px 15px;
            margin: 5px 0;
        }
        .stat-card {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border-radius: 15px;
            padding: 25px;
            text-align: center;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
        }
        .stat-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 10px 30px rgba(0,0,0,0.2);
        }
        .stat-card.green {
            background: linear-gradient(135deg, #28a745 0%, #20c997 10%);
        }
        .stat-card.orange {
            background: linear-gradient(135deg, #fd7e14 0%, #ffc107 100%);
        }
        .stat-card.red {
            background: linear-gradient(135deg, #dc3545 0%, #fd7e14 100%);
        }
        .stat-number {
            font-size: 2.5rem;
            font-weight: bold;
        }
        .rank-item {
            display: flex;
            align-items: center;
            padding: 15px;
            border-bottom: 1px solid #eee;
            transition: background 0.3s;
        }
        .rank-item:hover {
            background: #f8f9fa;
        }
        .rank-number {
            width: 35px;
            height: 35px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: bold;
            margin-right: 15px;
        }
        .rank-1 { background: #ffd700; color: white; }
        .rank-2 { background: #c0c0c0; color: white; }
        .rank-3 { background: #cd7f32; color: white; }
        .rank-other { background: #e9ecef; color: #6c757d; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container-fluid">
            <div class="row">
                <nav class="col-md-2 sidebar py-4" id="sidebar">
                    <div class="text-center mb-4">
                        <i class="fas fa-chalkboard-teacher fa-2x mb-2"></i>
                        <h5>请假系统</h5>
                        <small>教师端</small>
                    </div>
                    <ul class="nav flex-column px-3">
                        <li class="nav-item">
                            <a class="nav-link active" href="#" onclick="showSection('dashboard', event)">
                                <i class="fas fa-chart-line me-2"></i>数据概览
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="#" onclick="showSection('pending', event)">
                                <i class="fas fa-clock me-2"></i>待审批
                                <asp:Label ID="lblPendingCount" runat="server" CssClass="badge bg-warning float-end"></asp:Label>
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="#" onclick="showSection('history', event)">
                                <i class="fas fa-history me-2"></i>审批记录
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="#" onclick="showSection('classStats', event)">
                                <i class="fas fa-users me-2"></i>班级统计
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="#" onclick="showSection('studentStats', event)">
                                <i class="fas fa-user-graduate me-2"></i>学生统计
                            </a>
                        </li>
                        <li class="nav-item mt-4">
                            <a class="nav-link" href="../Default.aspx">
                                <i class="fas fa-sign-out-alt me-2"></i>退出登录
                            </a>
                        </li>
                    </ul>
                </nav>

                <main class="col-md-10 ms-sm-auto px-4 py-4">
                    <div class="d-flex justify-content-between align-items-center mb-4">
                        <h4><i class="fas fa-user-tie me-2"></i>欢迎，<span id="userName"><asp:Label ID="lblUserName" runat="server"></asp:Label></span></h4>
                        <button class="btn btn-outline-success d-md-none" onclick="toggleSidebar()">
                            <i class="fas fa-bars"></i>
                        </button>
                    </div>

                    <!-- 数据概览 -->
                    <div id="dashboardSection">
                        <div class="row g-3 mb-4">
                            <div class="col-md-3">
                                <div class="stat-card" onclick="showSection('classStats', event)" title="点击查看班级统计">
                                    <i class="fas fa-calendar-check fa-2x mb-3"></i>
                                    <asp:Label ID="lblTotalLeaves" runat="server" CssClass="stat-number"></asp:Label>
                                    <div>总请假数</div>
                                    <small style="opacity:0.7;font-size:0.75rem;">点击查看统计</small>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="stat-card green" onclick="showSection('pending', event)" title="点击去审批">
                                    <i class="fas fa-clock fa-2x mb-3"></i>
                                    <asp:Label ID="lblTeacherPending" runat="server" CssClass="stat-number" Text="0"></asp:Label>
                                    <div>待审批</div>
                                    <small style="opacity:0.7;font-size:0.75rem;">点击去审批</small>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="stat-card orange" onclick="showSection('historyApproved', event)" title="点击查看已通过记录">
                                    <i class="fas fa-check-circle fa-2x mb-3"></i>
                                    <asp:Label ID="lblTeacherApproved" runat="server" CssClass="stat-number"></asp:Label>
                                    <div>已通过</div>
                                    <small style="opacity:0.7;font-size:0.75rem;">点击查看记录</small>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="stat-card red" onclick="showSection('historyRejected', event)" title="点击查看已拒绝记录">
                                    <i class="fas fa-times-circle fa-2x mb-3"></i>
                                    <asp:Label ID="lblTeacherRejected" runat="server" CssClass="stat-number"></asp:Label>
                                    <div>已拒绝</div>
                                    <small style="opacity:0.7;font-size:0.75rem;">点击查看记录</small>
                                </div>
                            </div>
                        </div>

                        <div class="row g-3">
                            <div class="col-md-6">
                                <div class="card">
                                    <div class="card-header bg-white py-3">
                                        <h5 class="mb-0"><i class="fas fa-users me-2 text-primary"></i>请假最多的班级</h5>
                                    </div>
                                    <div class="card-body">
                                        <asp:Repeater ID="rptClassRanking" runat="server">
                                            <ItemTemplate>
                                                <div class="rank-item">
                                                    <div class="rank-number <%# Container.ItemIndex < 3 ? "rank-" + (Container.ItemIndex + 1) : "rank-other" %>">
                                                        <%# Container.ItemIndex + 1 %>
                                                    </div>
                                                    <div class="flex-grow-1">
                                                        <div class="fw-bold"><%# Eval("class_name") %></div>
                                                        <small class="text-muted">事假 <%# Eval("事假") %> | 病假 <%# Eval("病假") %> | 其他 <%# Eval("其他") %></small>
                                                    </div>
                                                    <div class="text-end">
                                                        <span class="badge bg-primary"><%# Eval("total") %>次</span>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="card">
                                    <div class="card-header bg-white py-3 d-flex justify-content-between align-items-center">
                                        <h5 class="mb-0"><i class="fas fa-user-graduate me-2 text-primary"></i>请假最多的学生</h5>
                                        <div class="btn-group btn-group-sm" id="rankingSortGroup">
                                            <button type="button" class="btn btn-outline-primary active" onclick="sortRanking('total_count', this)">按次数</button>
                                            <button type="button" class="btn btn-outline-primary" onclick="sortRanking('total_days', this)">按天数</button>
                                        </div>
                                    </div>
                                    <div class="card-body" id="rankingContainer">
                                        <asp:Repeater ID="rptStudentRanking" runat="server">
                                            <ItemTemplate>
                                                <div class="rank-item">
                                                    <div class="rank-number <%# Container.ItemIndex < 3 ? "rank-" + (Container.ItemIndex + 1) : "rank-other" %>">
                                                        <%# Container.ItemIndex + 1 %>
                                                    </div>
                                                    <div class="flex-grow-1">
                                                        <div class="fw-bold"><%# Eval("student_name") %></div>
                                                        <small class="text-muted"><%# Eval("class_name") %> | 共<%# Eval("total_days") %>天</small>
                                                    </div>
                                                    <div class="text-end">
                                                        <span class="badge bg-primary"><%# Eval("total_count") %>次</span>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- 待审批 -->
                    <div id="pendingSection" style="display:none;">
                        <div class="card mb-4">
                            <div class="card-header bg-white py-3">
                                <h5 class="mb-0"><i class="fas fa-hourglass-half me-2 text-warning"></i>待审批请假</h5>
                            </div>
                            <div class="card-body">
                                <asp:Repeater ID="rptPendingLeaves" runat="server">
                                    <ItemTemplate>
                                        <div class="card approval-card mb-3" data-leave-id='<%# DataBinder.Eval(Container.DataItem, "leave_id") %>' onclick="showDetail(this.dataset.leaveId)" style="cursor:pointer;">
                                            <div class="card-body">
                                                <div class="d-flex justify-content-between align-items-start">
                                                    <div>
                                                        <h6 class="mb-1"><i class="fas fa-user me-2"></i><%# Eval("student_name") %></h6>
                                                        <p class="mb-1 text-muted"><small><%# Eval("reason").ToString().Substring(0, Math.Min(Eval("reason").ToString().Length, 50)) %><%# Eval("reason").ToString().Length > 50 ? "..." : "" %></small></p>
                                                        <div class="mt-2">
                                                            <span class="badge bg-secondary me-2"><%# Eval("leave_type") %></span>
                                                            <span class="badge bg-info"><%# Eval("leave_days") %> 天</span>
                                                            <%# (int)Eval("leave_days") > 3 ? "<span class='badge bg-danger ms-2'>需领导审批</span>" : "" %>
                                                        </div>
                                                    </div>
                                                    <div class="text-end">
                                                        <small class="text-muted"><%# Eval("start_date") %> 至 <%# Eval("end_date") %></small>
                                                        <br />
                                                        <small class="text-muted"><%# Eval("create_time") %></small>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>

                    <!-- 审批记录 -->
                    <div id="historySection" style="display:none;">
                        <div class="card">
                            <div class="card-header bg-white py-3 d-flex justify-content-between align-items-center">
                                <h5 class="mb-0"><i class="fas fa-history me-2 text-success"></i>我的审批记录</h5>
                                <div class="btn-group btn-group-sm" id="historyFilterGroup">
                                    <button type="button" class="btn btn-outline-primary active" onclick="loadHistory('teacher_approved', this)">已通过</button>
                                    <button type="button" class="btn btn-outline-danger" onclick="loadHistory('rejected', this)">已拒绝</button>
                                </div>
                            </div>
                            <div class="card-body" id="historyContainer">
                            </div>
                        </div>
                    </div>

                    <!-- 班级统计 -->
                    <div id="classStatsSection" style="display:none;">
                        <div class="card">
                            <div class="card-header bg-white py-3">
                                <h5 class="mb-0"><i class="fas fa-chart-bar me-2 text-primary"></i>班级请假统计</h5>
                            </div>
                            <div class="card-body">
                                <div class="table-responsive">
                                    <asp:GridView ID="gvClassStats" runat="server" class="table table-hover" AutoGenerateColumns="false">
                                        <Columns>
                                            <asp:TemplateField HeaderText="排名">
                                                <ItemTemplate>
                                                    <span class="badge <%# Container.DataItemIndex < 3 ? "bg-warning" : "bg-secondary" %>">
                                                        <%# Container.DataItemIndex + 1 %>
                                                    </span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="class_name" HeaderText="班级名称" />
                                            <asp:BoundField DataField="total" HeaderText="请假总数" />
                                            <asp:BoundField DataField="事假" HeaderText="事假" />
                                            <asp:BoundField DataField="病假" HeaderText="病假" />
                                            <asp:BoundField DataField="其他" HeaderText="其他" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- 学生统计 -->
                    <div id="studentStatsSection" style="display:none;">
                        <div class="card">
                            <div class="card-header bg-white py-3 d-flex justify-content-between align-items-center">
                                <h5 class="mb-0"><i class="fas fa-user-chart me-2 text-primary"></i>学生请假统计</h5>
                                <div class="btn-group btn-group-sm" id="statsSortGroup">
                                    <button type="button" class="btn btn-outline-primary active" onclick="sortStudentStats('total_count', this)">按次数排序</button>
                                    <button type="button" class="btn btn-outline-primary" onclick="sortStudentStats('total_days', this)">按天数排序</button>
                                </div>
                            </div>
                            <div class="card-body" id="statsTableContainer">
                                <div class="table-responsive">
                                    <asp:GridView ID="gvStudentStats" runat="server" class="table table-hover" AutoGenerateColumns="false">
                                        <Columns>
                                            <asp:TemplateField HeaderText="排名">
                                                <ItemTemplate>
                                                    <span class="badge <%# Container.DataItemIndex < 3 ? "bg-warning" : "bg-secondary" %>">
                                                        <%# Container.DataItemIndex + 1 %>
                                                    </span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="student_name" HeaderText="学生姓名" />
                                            <asp:BoundField DataField="class_name" HeaderText="班级" />
                                            <asp:BoundField DataField="total_count" HeaderText="请假次数" />
                                            <asp:BoundField DataField="total_days" HeaderText="总天数" />
                                            <asp:BoundField DataField="事假" HeaderText="事假次数" />
                                            <asp:BoundField DataField="病假" HeaderText="病假次数" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </div>
                </main>
            </div>
        </div>

        <div class="modal fade" id="approvalModal" tabindex="-1">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title"><i class="fas fa-clipboard-check me-2"></i>请假详情</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <asp:HiddenField ID="hfLeaveId" runat="server" />
                        <div id="modalBody">
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="detail-row">
                                        <label class="text-muted small">学生姓名</label>
                                        <div class="fw-bold" id="studentName"></div>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="detail-row">
                                        <label class="text-muted small">联系电话</label>
                                        <div class="fw-bold" id="phone"></div>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="detail-row">
                                        <label class="text-muted small">请假类型</label>
                                        <div class="fw-bold" id="leaveType"></div>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="detail-row">
                                        <label class="text-muted small">请假天数</label>
                                        <div class="fw-bold" id="leaveDays"></div>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="detail-row">
                                        <label class="text-muted small">日期范围</label>
                                        <div class="fw-bold" id="dateRange"></div>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="detail-row">
                                        <label class="text-muted small">请假原因</label>
                                        <div class="fw-bold" id="reason"></div>
                                    </div>
                                </div>
                                <div class="col-12">
                                    <label class="form-label">审批意见</label>
                                    <asp:TextBox ID="txtApproveComment" runat="server" class="form-control" TextMode="MultiLine" Rows="2" placeholder="请输入审批意见（可选）"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-danger" onclick="rejectLeave()">
                            <i class="fas fa-times me-2"></i>拒绝
                        </button>
                        <button type="button" class="btn btn-success" onclick="approveLeave()">
                            <i class="fas fa-check me-2"></i>批准
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </form>
    <script src="https://cdn.bootcdn.net/ajax/libs/bootstrap/5.1.3/js/bootstrap.bundle.min.js"></script>
    <script>
        let approvalModal = null;

        function showSection(section, evt) {
            // 处理从仪表盘点击"已通过"/"已拒绝"跳转到审批记录的情况
            var actualSection = section;
            var historyFilter = null;
            if (section === 'historyApproved') {
                actualSection = 'history';
                historyFilter = 'teacher_approved';
            } else if (section === 'historyRejected') {
                actualSection = 'history';
                historyFilter = 'rejected';
            }

            document.getElementById('dashboardSection').style.display = actualSection === 'dashboard' ? 'block' : 'none';
            document.getElementById('pendingSection').style.display = actualSection === 'pending' ? 'block' : 'none';
            document.getElementById('historySection').style.display = actualSection === 'history' ? 'block' : 'none';
            document.getElementById('classStatsSection').style.display = actualSection === 'classStats' ? 'block' : 'none';
            document.getElementById('studentStatsSection').style.display = actualSection === 'studentStats' ? 'block' : 'none';

            document.querySelectorAll('.nav-link').forEach(link => link.classList.remove('active'));
            const sectionMap = { dashboard: 0, pending: 1, history: 2, classStats: 3, studentStats: 4 };
            const navLinks = document.querySelectorAll('.nav-link');
            if (navLinks[sectionMap[actualSection]]) navLinks[sectionMap[actualSection]].classList.add('active');

            if (actualSection === 'history') {
                var filterBtn = null;
                if (historyFilter) {
                    // 根据筛选值找到对应的按钮
                    var btns = document.querySelectorAll('#historyFilterGroup .btn');
                    btns.forEach(function(b) { b.classList.remove('active'); });
                    if (historyFilter === 'rejected') {
                        filterBtn = btns[1]; // 已拒绝按钮
                    } else {
                        filterBtn = btns[0]; // 已通过按钮
                    }
                    filterBtn.classList.add('active');
                } else {
                    filterBtn = document.querySelector('#historyFilterGroup .btn.active') || document.querySelector('#historyFilterGroup .btn');
                }
                var loadFilter = historyFilter || 'teacher_approved';
                loadHistory(loadFilter, filterBtn);
            }
        }

        function toggleSidebar() {
            document.getElementById('sidebar').classList.toggle('show');
        }

        document.addEventListener('DOMContentLoaded', function() {
            approvalModal = new bootstrap.Modal(document.getElementById('approvalModal'));
        });

        function showDetail(id) {
            document.getElementById('<%= hfLeaveId.ClientID %>').value = id;
            fetch('TeacherMain.aspx?getDetail=1&leaveId=' + id)
                .then(function(response) { return response.json(); })
                .then(function(data) {
                    document.getElementById('studentName').textContent = data.studentName || '';
                    document.getElementById('phone').textContent = data.phone || '';
                    document.getElementById('leaveType').textContent = data.leaveType || '';
                    document.getElementById('leaveDays').textContent = data.leaveDays ? data.leaveDays + ' 天' : '';
                    document.getElementById('dateRange').textContent = data.startDate + ' 至 ' + data.endDate;
                    document.getElementById('reason').innerHTML = data.reason || '';
                    approvalModal.show();
                })
                .catch(function() {
                    approvalModal.show();
                });
        }

        function approveLeave() {
            const leaveId = document.getElementById('<%= hfLeaveId.ClientID %>').value;
            const comment = document.getElementById('<%= txtApproveComment.ClientID %>').value;
            window.location.href = 'TeacherMain.aspx?action=approve&leaveId=' + leaveId + '&comment=' + encodeURIComponent(comment);
        }

        function rejectLeave() {
            const leaveId = document.getElementById('<%= hfLeaveId.ClientID %>').value;
            const comment = document.getElementById('<%= txtApproveComment.ClientID %>').value;
            window.location.href = 'TeacherMain.aspx?action=reject&leaveId=' + leaveId + '&comment=' + encodeURIComponent(comment);
        }

        // 排序切换 - 学生排名（AJAX 无刷新）
        function sortRanking(orderBy, btn) {
            document.querySelectorAll('#rankingSortGroup .btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            fetch('TeacherMain.aspx?ajaxSort=ranking&orderBy=' + orderBy)
                .then(function(r) { return r.json(); })
                .then(function(data) { renderStudentRanking(data); })
                .catch(function() {});
        }

        // 排序切换 - 学生统计（AJAX 无刷新）
        function sortStudentStats(orderBy, btn) {
            document.querySelectorAll('#statsSortGroup .btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            fetch('TeacherMain.aspx?ajaxSort=studentStats&orderBy=' + orderBy)
                .then(function(r) { return r.json(); })
                .then(function(data) { renderStudentStatsTable(data); })
                .catch(function() {});
        }

        // 动态渲染学生排名
        function renderStudentRanking(data) {
            var html = '';
            for (var i = 0; i < data.length; i++) {
                var d = data[i];
                var rankClass = i < 3 ? 'rank-' + (i + 1) : 'rank-other';
                html += '<div class="rank-item">'
                    + '<div class="rank-number ' + rankClass + '">' + (i + 1) + '</div>'
                    + '<div class="flex-grow-1">'
                    + '<div class="fw-bold">' + d.student_name + '</div>'
                    + '<small class="text-muted">' + d.class_name + ' | 共' + d.total_days + '天</small>'
                    + '</div>'
                    + '<div class="text-end"><span class="badge bg-primary">' + d.total_count + '次</span></div>'
                    + '</div>';
            }
            document.getElementById('rankingContainer').innerHTML = html;
        }

        // 动态渲染学生统计表格
        function renderStudentStatsTable(data) {
            var html = '<table class="table table-hover"><thead><tr>'
                + '<th>排名</th><th>学生姓名</th><th>班级</th><th>请假次数</th><th>总天数</th><th>事假次数</th><th>病假次数</th>'
                + '</tr></thead><tbody>';
            for (var i = 0; i < data.length; i++) {
                var d = data[i];
                var badgeClass = i < 3 ? 'bg-warning' : 'bg-secondary';
                html += '<tr>'
                    + '<td><span class="badge ' + badgeClass + '">' + (i + 1) + '</span></td>'
                    + '<td>' + d.student_name + '</td>'
                    + '<td>' + d.class_name + '</td>'
                    + '<td>' + d.total_count + '</td>'
                    + '<td>' + d.total_days + '</td>'
                    + '<td>' + d.shijia + '</td>'
                    + '<td>' + d.bingjia + '</td>'
                    + '</tr>';
            }
            html += '</tbody></table>';
            document.getElementById('statsTableContainer').innerHTML = html;
        }

        // 加载审批记录
        function loadHistory(statusFilter, btn) {
            document.querySelectorAll('#historyFilterGroup .btn').forEach(function(b) { b.classList.remove('active'); });
            if (btn) btn.classList.add('active');
            var container = document.getElementById('historyContainer');
            container.innerHTML = '<div class="text-center text-muted py-4"><i class="fas fa-spinner fa-spin me-2"></i>加载中...</div>';
            fetch('TeacherMain.aspx?ajaxHistory=1&statusFilter=' + statusFilter)
                .then(function(r) { return r.json(); })
                .then(function(data) { renderHistory(data, statusFilter); })
                .catch(function() { container.innerHTML = '<div class="text-center text-danger py-4">加载失败</div>'; });
        }

        // 渲染审批记录
        function renderHistory(data, statusFilter) {
            var container = document.getElementById('historyContainer');
            if (!data || data.length === 0) {
                container.innerHTML = '<div class="text-center text-muted py-4"><i class="fas fa-inbox fa-2x mb-2"></i><br>暂无记录</div>';
                return;
            }
            var html = '';
            for (var i = 0; i < data.length; i++) {
                var d = data[i];
                var borderColor = statusFilter === 'rejected' ? '#dc3545' : '#28a745';
                var statusBadge = statusFilter === 'rejected'
                    ? '<span class="badge bg-danger">已拒绝</span>'
                    : '<span class="badge bg-success">已通过</span>';
                html += '<div class="card mb-3" style="border-left:4px solid ' + borderColor + ';">'
                    + '<div class="card-body">'
                    + '<div class="d-flex justify-content-between align-items-start">'
                    + '<div>'
                    + '<h6 class="mb-1"><i class="fas fa-user me-2"></i>' + d.student_name + ' <small class="text-muted">(' + d.class_name + ')</small></h6>'
                    + '<p class="mb-1 text-muted"><small>' + d.reason + '</small></p>'
                    + '<div class="mt-2">'
                    + '<span class="badge bg-secondary me-2">' + d.leave_type + '</span>'
                    + '<span class="badge bg-info">' + d.leave_days + ' 天</span>'
                    + statusBadge
                    + '</div>'
                    + '</div>'
                    + '<div class="text-end">'
                    + '<small class="text-muted">' + d.start_date + ' 至 ' + d.end_date + '</small>'
                    + '<br><small class="text-muted">' + d.approve_time + '</small>'
                    + '</div>'
                    + '</div>'
                    + (d.approve_comment ? '<div class="mt-2 p-2 bg-light rounded"><small class="text-muted">审批意见：</small> ' + d.approve_comment + '</div>' : '')
                    + '</div></div>';
            }
            container.innerHTML = html;
        }
    </script>
</body>
</html>
