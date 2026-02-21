namespace SaasSystem.Permissions;

public static class SaasSystemPermissions
{
    public const string GroupName = "SaasSystem";

    public static class Dashboard
    {
        public const string Default = GroupName + ".Dashboard";
    }

    public static class Users
    {
        public const string Default = GroupName + ".Users";
    }

    public static class Orders
    {
        public const string Default = GroupName + ".Orders";
    }

    public static class Settings
    {
        public const string Default = GroupName + ".Settings";
    }

    public static class Customers
    {
        public const string Default = GroupName + ".Customers";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Projects
    {
        public const string Default = GroupName + ".Projects";
    }

    public static class Tasks
    {
        public const string Default = GroupName + ".Tasks";
        public const string Move = Default + ".Move";
    }

    public static class Invoices
    {
        public const string Default = GroupName + ".Invoices";
    }

    public static class Payments
    {
        public const string Default = GroupName + ".Payments";
    }

    public static class Reports
    {
        public const string Default = GroupName + ".Reports";
    }

    public static class Administration
    {
        public const string Default = GroupName + ".Administration";
    }
}
