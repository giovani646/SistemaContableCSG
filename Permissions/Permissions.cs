namespace SistemaContableCSG.Permissions
{
    public class Permissions
    {
        public static class GestionarUsuarios
        {
            public const string View = "Permissions.GestionarUsuarios.Ver";
            public const string Create = "Permissions.GestionarUsuarios.Crear";
            public const string Edit = "Permissions.GestionarUsuarios.Editar";
            public const string Delete = "Permissions.GestionarUsuarios.Eliminar";
            public const string BlockAndUnclock = "Permissions.GestionarUsuarios.Bloquear&Desbloquear";
        }

        public static class GestionarRoles
        {
            public const string View = "Permissions.GestionarRoles.Ver";
            public const string Create = "Permissions.GestionarRoles.Crear";
            public const string Edit = "Permissions.GestionarRoles.Editar";
            public const string Delete = "Permissions.GestionarRoles.Eliminar";
        }
    }
}
