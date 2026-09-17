namespace TaskTrackerSystem.Domain.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Director = "Director";         
    public const string DepartmentManager = "DepartmentManager";                                                  
    public const string Employee = "Employee";

    public static readonly string[] All = { Admin, Director, DepartmentManager };

    public const string ManagerAndDirector = DepartmentManager + "," + Director;
    public const string AllRolesCombined = Admin + ", " + Director + ", " + DepartmentManager;

    public static readonly string[] ManagementRoles = { Director, DepartmentManager };
}