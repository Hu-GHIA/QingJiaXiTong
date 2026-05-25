<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Default.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>高校请假系统 - 登录</title>
    <link href="https://cdn.bootcdn.net/ajax/libs/bootstrap/5.1.3/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.bootcdn.net/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    <style>
        body {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .login-container {
            background: white;
            border-radius: 15px;
            box-shadow: 0 15px 35px rgba(0,0,0,0.2);
            overflow: hidden;
            max-width: 450px;
            width: 100%;
        }
        .login-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }
        .login-body {
            padding: 40px 30px;
        }
        .form-control:focus {
            border-color: #667eea;
            box-shadow: 0 0 0 0.2rem rgba(102, 126, 234, 0.25);
        }
        .btn-login {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border: none;
            color: white;
            padding: 12px;
            font-weight: 600;
        }
        .btn-login:hover {
            background: linear-gradient(135deg, #5a6fd6 0%, #6a4190 100%);
            color: white;
        }
        .role-indicator {
            position: absolute;
            right: 15px;
            top: 50%;
            transform: translateY(-50%);
            color: #667eea;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <div class="login-header">
                <h2><i class="fas fa-calendar-check me-2"></i>高校请假系统</h2>
                <p class="mb-0">二级学院请假管理平台</p>
            </div>
            <div class="login-body">
                <div class="mb-4">
                    <label class="form-label">用户名</label>
                    <div class="position-relative">
                        <asp:TextBox ID="txtUsername" runat="server" class="form-control" placeholder="请输入用户名" required></asp:TextBox>
                        <i class="fas fa-user role-indicator"></i>
                    </div>
                </div>
                <div class="mb-4">
                    <label class="form-label">密码</label>
                    <div class="position-relative">
                        <asp:TextBox ID="txtPassword" runat="server" class="form-control" TextMode="Password" placeholder="请输入密码" required></asp:TextBox>
                        <i class="fas fa-lock role-indicator"></i>
                    </div>
                </div>
                <asp:Button ID="btnLogin" runat="server" class="btn btn-login w-100" Text="登录" OnClick="btnLogin_Click" />
                <div class="mt-4 text-center text-muted">
                    <small>系统将根据账号自动识别身份并跳转</small><br />
                    <small>演示账号：stu001 / 123456（学生）</small><br />
                    <small>演示账号：tea001 / 123456（老师）</small><br />
                    <small>演示账号：lea001 / 123456（领导）</small>
                </div>
                <div class="text-danger d-block mt-3"><asp:Literal ID="litError" runat="server"></asp:Literal></div>
            </div>
        </div>
    </form>
    <script src="https://cdn.bootcdn.net/ajax/libs/bootstrap/5.1.3/js/bootstrap.bundle.min.js"></script>
</body>
</html>