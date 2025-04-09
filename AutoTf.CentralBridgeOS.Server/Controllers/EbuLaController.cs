using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

/// <summary>
/// This controller features the following features:
/// <para>/currentTable - Returns the current timetable.</para>
/// <para>/edit - Creates/overwrites/edits the current timetable.</para>
/// <para>/currentConditions - Returns the current conditions based on the current location. (E.g. Speed limit, next stop, etc.)</para>
/// <para>/disableAutoDetection - Disables the auto detection of EbuLa changes for this session.
/// (TODO: does this even ever happen? But it could disable the "validation" of the EbuLa while driving)</para>
/// <para>/disableLocalisation - Disables the localisation using the location marker on the EbuLa device for this session.</para>
/// <para>/turnOffDetection - Permanently turns off the auto detection of a timetable from the EbuLa.
/// (TODO: Although we will probably still want to enter the train number into the device)</para>
/// <para>/turnOffLocalisation - Permanently turns off the localisation using the location marker on the EbuLa.</para>
/// <para>/enableAutoDetection - Re-enables the auto detection of the timetable using the EbuLa. This invokes a full re scan of the current timetable.</para>
/// <para>/enableLocalisation - Re-enables the localisation using the location marker on the EbuLa device. This invokes a jump to the current page on the EbuLa.</para>
/// <para>/turnOnAutoDetection - Turns on the auto detection of a timetable on startup back on.</para>
/// <para>/turnOnLocalisation - Turns on the localisation using the location marker on the Ebula. This invokes a jump to the current page on the EbuLa.</para>
/// <para>/rescan - Rescans the entire timetable from the EbuLa device and returns it.</para>
/// <remarks>Documented in OpenApi</remarks>
/// </summary>
[ApiController]
[Route("ebula")]
public class EbuLaController : ControllerBase
{
    
}