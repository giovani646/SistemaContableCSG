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

        public static class GestionarPeriodos
        {
            public const string View = "Permissions.GestionarPeriodos.Ver";
            public const string Create = "Permissions.GestionarPeriodos.Crear";
            public const string ActivarDesactivar = "Permissions.GestionarPeriodos.Activar&Desactivar";
        }

        public static class GenerarReporte
        {
            public const string View = "Permissions.GenerarReporte.Ver";
            public const string CatalogoDeCuentas = "Permissions.GenerarReporte.CatalogoDeCuentas";
            public const string BalanceGeneral = "Permissions.GenerarReporte.BalanceGeneral";
            public const string TrasactTipoSald = "Permissions.GenerarReporte.TrasactTipoSald";
            public const string LibroDiario = "Permissions.GenerarReporte.LibroDiario";
        }

        public static class GestionarAsientos
        {
            public const string View = "Permissions.GestionarAsientos.Ver";
            public const string Create = "Permissions.GestionarAsientos.Crear";
        }

        public static class GestionarCuentas
        {
            public const string View = "Permissions.GestionarCuentas.Ver";
            public const string Create = "Permissions.GestionarCuentas.Crear";
            public const string Edit = "Permissions.GestionarCuentas.Editar";
            public const string Delete = "Permissions.GestionarCuentas.Eliminar";
        }

        public static class Bitacora
        {
            public const string View = "Permissions.Bitacora.Ver";
        }
    }
}
