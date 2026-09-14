using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-050. Alcance por empresa (06-seguridad.md §11): un usuario solo puede operar sobre
// Requisicion de su propia Empresa. Reutiliza Usuario.Empresa (ya existente, Empresa 1--N
// Usuario) — no se crea ninguna entidad de alcance nueva (TASK-050 §4/§30). Alcance por sede NO
// se implementa aquí: Requisicion no tiene un único SedeId (puede distribuirse entre varias
// sedes vía DistribucionRequisicion). Esto planteó una ambigüedad real (¿todas las sedes
// tocadas, o alguna?) que se presentó al usuario explícitamente — decidió posponerla y mantener
// alcance únicamente por Empresa (RN-058, 06-seguridad.md §13). No es una omisión.
//
// Si la Requisicion no existe, se devuelve true (permite continuar) para que el 404 real lo
// reporte el caso de uso correspondiente (RequisicionFinder.ObtenerOLanzar) — este chequeo de
// alcance no debe enmascarar "no existe" detrás de "sin autorización".
public sealed class UsuarioTieneAlcanceSobreRequisicionUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IRequisicionRepository _requisiciones;

    public UsuarioTieneAlcanceSobreRequisicionUseCase(IUsuarioRepository usuarios, IRequisicionRepository requisiciones)
    {
        _usuarios = usuarios;
        _requisiciones = requisiciones;
    }

    public bool Ejecutar(int usuarioId, int requisicionId)
    {
        var requisicion = _requisiciones.ObtenerPorId(requisicionId);
        if (requisicion is null)
            return true;

        var usuario = _usuarios.ObtenerPorId(usuarioId);
        if (usuario is null)
            return false;

        return usuario.Empresa.Id == requisicion.Empresa.Id;
    }
}
