# 高校请假系统

基于 ASP.NET Web Forms 的二级学院请假管理平台，支持学生在线请假、班主任审批、学院领导终审的完整流程。

## 功能概览

### 统一登录
- 用户使用账号密码登录，系统根据角色自动跳转到对应工作台
- 三种角色：**学生**（student）、**班主任**（teacher）、**学院领导**（leader）

### 学生端
- 在线提交请假申请（请假类型、起止日期、请假天数、事由、联系电话）
- 查看个人请假记录及审批状态
- 取消待审批的请假申请

### 班主任端（教师）
- **待审批列表** — 查看并审批/拒绝学生的请假申请
- **班级排名** — 按请假次数查看班级排行榜（TOP 6）
- **学生排名** — 按请假次数/天数查看学生排行榜（TOP 6），支持排序切换
- **班级统计** — 各班按事假、病假、其他分类汇总
- **学生统计** — 每个学生的请假次数、天数、事假/病假分类统计
- **审批记录** — 查看本人已批准/已拒绝的历史记录

### 学院领导端
- 审批班主任已通过且请假天数 > 3 天的申请（二级审批）
- 全院数据概览（请假总数、待审批数、已通过/已拒绝数）
- 班级排名、学生排名（与教师端相同的统计视图）
- 审批记录查看

## 技术栈

| 层级 | 技术 |
|------|------|
| 框架 | ASP.NET Web Forms (.NET Framework 4.8) |
| 语言 | C# |
| 数据库 | SQL Server (CollegeLeaveSystem) |
| 前端 | Bootstrap 5.1.3, Font Awesome 6, jQuery |
| IDE | Visual Studio |

## 数据库结构

```
CollegeLeaveSystem
├── [user]           — 用户表（学生/班主任/领导）
├── [department]     — 院系/部门表
├── [class]          — 班级表
├── [leave_request]  — 请假申请表
└── [leave_log]      — 操作日志表
```

### 请假审批流程

```
学生提交 → 状态: pending
              ↓
        班主任审批 ──→ 拒绝 → 状态: rejected（结束）
              ↓
          通过 → 请假 ≤ 3天 → 状态: teacher_approved（结束）
              ↓
          通过 → 请假 > 3天 → 进入领导审批
              ↓
        领导审批 ──→ 拒绝 → 状态: rejected（结束）
              ↓
          通过 → 状态: leader_approved（结束）
```

## 快速开始

### 环境要求
- Visual Studio 2019+
- .NET Framework 4.8
- SQL Server (Express 或以上)
- IIS Express（开发环境自带）

### 配置步骤

1. **克隆或下载项目**

2. **恢复 NuGet 包**
   ```
   NuGet 包还原
   ```

3. **创建数据库**
   - 在 SQL Server 中执行 `sql脚本.sql` 创建数据库和表结构

4. **配置连接字符串**
   - 修改 `Web.config` 中的连接字符串 `LeaveSystemConnectionString`，指向你的 SQL Server 实例

5. **运行项目**
   - 在 Visual Studio 中打开解决方案，按 F5 运行

### 演示账号

| 账号 | 密码 | 角色 |
|------|------|------|
| stu001 | 123456 | 学生 |
| tea001 | 123456 | 班主任 |
| lea001 | 123456 | 学院领导 |

## 项目结构

```
请假系统/
├── Default.aspx              # 登录页面
├── Student/
│   └── StudentMain.aspx      # 学生工作台
├── Teacher/
│   └── TeacherMain.aspx      # 班主任工作台
├── Leader/
│   └── LeaderMain.aspx       # 领导工作台
├── Web.config                # 应用配置
├── sql脚本.sql                # 建库建表脚本
└── 请假系统.sln               # 解决方案文件
```

## License

本系统仅供学习与内部使用。
