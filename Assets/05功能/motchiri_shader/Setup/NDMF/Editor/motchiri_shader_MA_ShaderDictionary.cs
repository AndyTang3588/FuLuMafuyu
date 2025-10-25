using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace wataameya.motchiri_shader.ndmf.editor
{
    public class motchiri_shader_MA_ShaderDictionary
    {
        public Dictionary<string,string> liltoonToMotchiri = new Dictionary<string,string>();
        public Dictionary<string,string> liltoonToMotchiriTessellation = new Dictionary<string,string>();
        
        public motchiri_shader_MA_ShaderDictionary()
        {
            liltoonToMotchiri.Add(liltoon_lts         ,motchiri_lts        );
            liltoonToMotchiri.Add(liltoon_ltsc        ,motchiri_ltsc       );
            liltoonToMotchiri.Add(liltoon_ltst        ,motchiri_ltst       );
            liltoonToMotchiri.Add(liltoon_ltsot       ,motchiri_ltsot      );
            liltoonToMotchiri.Add(liltoon_ltstt       ,motchiri_ltstt      );
            liltoonToMotchiri.Add(liltoon_ltso        ,motchiri_ltso       );
            liltoonToMotchiri.Add(liltoon_ltsco       ,motchiri_ltsco      );
            liltoonToMotchiri.Add(liltoon_ltsto       ,motchiri_ltsto      );
            liltoonToMotchiri.Add(liltoon_ltsoto      ,motchiri_ltsoto     );
            liltoonToMotchiri.Add(liltoon_ltstto      ,motchiri_ltstto     );
            liltoonToMotchiri.Add(liltoon_ltsoo       ,motchiri_ltsoo      );
            liltoonToMotchiri.Add(liltoon_ltscoo      ,motchiri_ltscoo     );
            liltoonToMotchiri.Add(liltoon_ltstoo      ,motchiri_ltstoo     );
            liltoonToMotchiri.Add(liltoon_ltstess     ,motchiri_ltstess    );
            liltoonToMotchiri.Add(liltoon_ltstessc    ,motchiri_ltstessc   );
            liltoonToMotchiri.Add(liltoon_ltstesst    ,motchiri_ltstesst   );
            liltoonToMotchiri.Add(liltoon_ltstessot   ,motchiri_ltstessot  );
            liltoonToMotchiri.Add(liltoon_ltstesstt   ,motchiri_ltstesstt  );
            liltoonToMotchiri.Add(liltoon_ltstesso    ,motchiri_ltstesso   );
            liltoonToMotchiri.Add(liltoon_ltstessco   ,motchiri_ltstessco  );
            liltoonToMotchiri.Add(liltoon_ltstessto   ,motchiri_ltstessto  );
            liltoonToMotchiri.Add(liltoon_ltstessoto  ,motchiri_ltstessoto );
            liltoonToMotchiri.Add(liltoon_ltstesstto  ,motchiri_ltstesstto );
            liltoonToMotchiri.Add(liltoon_ltsl        ,motchiri_ltsl       );
            liltoonToMotchiri.Add(liltoon_ltslc       ,motchiri_ltslc      );
            liltoonToMotchiri.Add(liltoon_ltslt       ,motchiri_ltslt      );
            liltoonToMotchiri.Add(liltoon_ltslot      ,motchiri_ltslot     );
            liltoonToMotchiri.Add(liltoon_ltsltt      ,motchiri_ltsltt     );
            liltoonToMotchiri.Add(liltoon_ltslo       ,motchiri_ltslo      );
            liltoonToMotchiri.Add(liltoon_ltslco      ,motchiri_ltslco     );
            liltoonToMotchiri.Add(liltoon_ltslto      ,motchiri_ltslto     );
            liltoonToMotchiri.Add(liltoon_ltsloto     ,motchiri_ltsloto    );
            liltoonToMotchiri.Add(liltoon_ltsltto     ,motchiri_ltsltto    );
            liltoonToMotchiri.Add(liltoon_ltsref      ,motchiri_ltsref     );
            liltoonToMotchiri.Add(liltoon_ltsrefb     ,motchiri_ltsrefb    );
            liltoonToMotchiri.Add(liltoon_ltsfur      ,motchiri_ltsfur     );
            liltoonToMotchiri.Add(liltoon_ltsfurc     ,motchiri_ltsfurc    );
            liltoonToMotchiri.Add(liltoon_ltsfurtwo   ,motchiri_ltsfurtwo  );
            liltoonToMotchiri.Add(liltoon_ltsfuro     ,motchiri_ltsfuro    );
            liltoonToMotchiri.Add(liltoon_ltsfuroc    ,motchiri_ltsfuroc   );
            liltoonToMotchiri.Add(liltoon_ltsfurotwo  ,motchiri_ltsfurotwo );
            liltoonToMotchiri.Add(liltoon_ltsgem      ,motchiri_ltsgem     );
            liltoonToMotchiri.Add(liltoon_ltsfs       ,motchiri_ltsfs      );
            liltoonToMotchiri.Add(liltoon_ltsover     ,motchiri_ltsover    );
            liltoonToMotchiri.Add(liltoon_ltsoover    ,motchiri_ltsoover   );
            liltoonToMotchiri.Add(liltoon_ltslover    ,motchiri_ltslover   );
            liltoonToMotchiri.Add(liltoon_ltsloover   ,motchiri_ltsloover  );
            liltoonToMotchiri.Add(liltoon_ltsm        ,motchiri_ltsm       );
            liltoonToMotchiri.Add(liltoon_ltsmo       ,motchiri_ltsmo      );
            liltoonToMotchiri.Add(liltoon_ltsmref     ,motchiri_ltsmref    );
            liltoonToMotchiri.Add(liltoon_ltsmfur     ,motchiri_ltsmfur    );
            liltoonToMotchiri.Add(liltoon_ltsmgem     ,motchiri_ltsmgem    );

            liltoonToMotchiriTessellation.Add(liltoon_lts         ,motchiri_ltstess    );
            liltoonToMotchiriTessellation.Add(liltoon_ltsc        ,motchiri_ltstessc   );
            liltoonToMotchiriTessellation.Add(liltoon_ltst        ,motchiri_ltstesst   );
            liltoonToMotchiriTessellation.Add(liltoon_ltsot       ,motchiri_ltstessot  );
            liltoonToMotchiriTessellation.Add(liltoon_ltstt       ,motchiri_ltstesstt  );
            liltoonToMotchiriTessellation.Add(liltoon_ltso        ,motchiri_ltstesso   );
            liltoonToMotchiriTessellation.Add(liltoon_ltsco       ,motchiri_ltstessco  );
            liltoonToMotchiriTessellation.Add(liltoon_ltsto       ,motchiri_ltstessto  );
            liltoonToMotchiriTessellation.Add(liltoon_ltsoto      ,motchiri_ltstessoto );
            liltoonToMotchiriTessellation.Add(liltoon_ltstto      ,motchiri_ltstesstto );
            liltoonToMotchiriTessellation.Add(liltoon_ltsoo       ,motchiri_ltsoo      );
            liltoonToMotchiriTessellation.Add(liltoon_ltscoo      ,motchiri_ltscoo     );
            liltoonToMotchiriTessellation.Add(liltoon_ltstoo      ,motchiri_ltstoo     );
            liltoonToMotchiriTessellation.Add(liltoon_ltstess     ,motchiri_ltstess    );
            liltoonToMotchiriTessellation.Add(liltoon_ltstessc    ,motchiri_ltstessc   );
            liltoonToMotchiriTessellation.Add(liltoon_ltstesst    ,motchiri_ltstesst   );
            liltoonToMotchiriTessellation.Add(liltoon_ltstessot   ,motchiri_ltstessot  );
            liltoonToMotchiriTessellation.Add(liltoon_ltstesstt   ,motchiri_ltstesstt  );
            liltoonToMotchiriTessellation.Add(liltoon_ltstesso    ,motchiri_ltstesso   );
            liltoonToMotchiriTessellation.Add(liltoon_ltstessco   ,motchiri_ltstessco  );
            liltoonToMotchiriTessellation.Add(liltoon_ltstessto   ,motchiri_ltstessto  );
            liltoonToMotchiriTessellation.Add(liltoon_ltstessoto  ,motchiri_ltstessoto );
            liltoonToMotchiriTessellation.Add(liltoon_ltstesstto  ,motchiri_ltstesstto );
            liltoonToMotchiriTessellation.Add(liltoon_ltsl        ,motchiri_ltsl       );
            liltoonToMotchiriTessellation.Add(liltoon_ltslc       ,motchiri_ltslc      );
            liltoonToMotchiriTessellation.Add(liltoon_ltslt       ,motchiri_ltslt      );
            liltoonToMotchiriTessellation.Add(liltoon_ltslot      ,motchiri_ltslot     );
            liltoonToMotchiriTessellation.Add(liltoon_ltsltt      ,motchiri_ltsltt     );
            liltoonToMotchiriTessellation.Add(liltoon_ltslo       ,motchiri_ltslo      );
            liltoonToMotchiriTessellation.Add(liltoon_ltslco      ,motchiri_ltslco     );
            liltoonToMotchiriTessellation.Add(liltoon_ltslto      ,motchiri_ltslto     );
            liltoonToMotchiriTessellation.Add(liltoon_ltsloto     ,motchiri_ltsloto    );
            liltoonToMotchiriTessellation.Add(liltoon_ltsltto     ,motchiri_ltsltto    );
            liltoonToMotchiriTessellation.Add(liltoon_ltsref      ,motchiri_ltsref     );
            liltoonToMotchiriTessellation.Add(liltoon_ltsrefb     ,motchiri_ltsrefb    );
            liltoonToMotchiriTessellation.Add(liltoon_ltsfur      ,motchiri_ltsfur     );
            liltoonToMotchiriTessellation.Add(liltoon_ltsfurc     ,motchiri_ltsfurc    );
            liltoonToMotchiriTessellation.Add(liltoon_ltsfurtwo   ,motchiri_ltsfurtwo  );
            liltoonToMotchiriTessellation.Add(liltoon_ltsfuro     ,motchiri_ltsfuro    );
            liltoonToMotchiriTessellation.Add(liltoon_ltsfuroc    ,motchiri_ltsfuroc   );
            liltoonToMotchiriTessellation.Add(liltoon_ltsfurotwo  ,motchiri_ltsfurotwo );
            liltoonToMotchiriTessellation.Add(liltoon_ltsgem      ,motchiri_ltsgem     );
            liltoonToMotchiriTessellation.Add(liltoon_ltsfs       ,motchiri_ltsfs      );
            liltoonToMotchiriTessellation.Add(liltoon_ltsover     ,motchiri_ltsover    );
            liltoonToMotchiriTessellation.Add(liltoon_ltsoover    ,motchiri_ltsoover   );
            liltoonToMotchiriTessellation.Add(liltoon_ltslover    ,motchiri_ltslover   );
            liltoonToMotchiriTessellation.Add(liltoon_ltsloover   ,motchiri_ltsloover  );
            liltoonToMotchiriTessellation.Add(liltoon_ltsm        ,motchiri_ltsm       );
            liltoonToMotchiriTessellation.Add(liltoon_ltsmo       ,motchiri_ltsmo      );
            liltoonToMotchiriTessellation.Add(liltoon_ltsmref     ,motchiri_ltsmref    );
            liltoonToMotchiriTessellation.Add(liltoon_ltsmfur     ,motchiri_ltsmfur    );
            liltoonToMotchiriTessellation.Add(liltoon_ltsmgem     ,motchiri_ltsmgem    );
        }

        private string liltoon_lts         = ("lilToon");
        private string liltoon_ltsc        = ("Hidden/lilToonCutout");
        private string liltoon_ltst        = ("Hidden/lilToonTransparent");
        private string liltoon_ltsot       = ("Hidden/lilToonOnePassTransparent");
        private string liltoon_ltstt       = ("Hidden/lilToonTwoPassTransparent");

        private string liltoon_ltso        = ("Hidden/lilToonOutline");
        private string liltoon_ltsco       = ("Hidden/lilToonCutoutOutline");
        private string liltoon_ltsto       = ("Hidden/lilToonTransparentOutline");
        private string liltoon_ltsoto      = ("Hidden/lilToonOnePassTransparentOutline");
        private string liltoon_ltstto      = ("Hidden/lilToonTwoPassTransparentOutline");

        private string liltoon_ltsoo       = ("_lil/[Optional] lilToonOutlineOnly");
        private string liltoon_ltscoo      = ("_lil/[Optional] lilToonCutoutOutlineOnly");
        private string liltoon_ltstoo      = ("_lil/[Optional] lilToonTransparentOutlineOnly");

        private string liltoon_ltstess     = ("Hidden/lilToonTessellation");
        private string liltoon_ltstessc    = ("Hidden/lilToonTessellationCutout");
        private string liltoon_ltstesst    = ("Hidden/lilToonTessellationTransparent");
        private string liltoon_ltstessot   = ("Hidden/lilToonTessellationOnePassTransparent");
        private string liltoon_ltstesstt   = ("Hidden/lilToonTessellationTwoPassTransparent");

        private string liltoon_ltstesso    = ("Hidden/lilToonTessellationOutline");
        private string liltoon_ltstessco   = ("Hidden/lilToonTessellationCutoutOutline");
        private string liltoon_ltstessto   = ("Hidden/lilToonTessellationTransparentOutline");
        private string liltoon_ltstessoto  = ("Hidden/lilToonTessellationOnePassTransparentOutline");
        private string liltoon_ltstesstto  = ("Hidden/lilToonTessellationTwoPassTransparentOutline");

        private string liltoon_ltsl        = ("Hidden/lilToonLite");
        private string liltoon_ltslc       = ("Hidden/lilToonLiteCutout");
        private string liltoon_ltslt       = ("Hidden/lilToonLiteTransparent");
        private string liltoon_ltslot      = ("Hidden/lilToonLiteOnePassTransparent");
        private string liltoon_ltsltt      = ("Hidden/lilToonLiteTwoPassTransparent");

        private string liltoon_ltslo       = ("Hidden/lilToonLiteOutline");
        private string liltoon_ltslco      = ("Hidden/lilToonLiteCutoutOutline");
        private string liltoon_ltslto      = ("Hidden/lilToonLiteTransparentOutline");
        private string liltoon_ltsloto     = ("Hidden/lilToonLiteOnePassTransparentOutline");
        private string liltoon_ltsltto     = ("Hidden/lilToonLiteTwoPassTransparentOutline");

        private string liltoon_ltsref      = ("Hidden/lilToonRefraction");
        private string liltoon_ltsrefb     = ("Hidden/lilToonRefractionBlur");
        private string liltoon_ltsfur      = ("Hidden/lilToonFur");
        private string liltoon_ltsfurc     = ("Hidden/lilToonFurCutout");
        private string liltoon_ltsfurtwo   = ("Hidden/lilToonFurTwoPass");
        private string liltoon_ltsfuro     = ("_lil/[Optional] lilToonFurOnly");
        private string liltoon_ltsfuroc    = ("_lil/[Optional] lilToonFurOnlyCutout");
        private string liltoon_ltsfurotwo  = ("_lil/[Optional] lilToonFurOnlyTwoPass");

        private string liltoon_ltsgem      = ("Hidden/lilToonGem");

        private string liltoon_ltsfs       = ("_lil/lilToonFakeShadow");

        private string liltoon_ltsover     = ("_lil/[Optional] lilToonOverlay");
        private string liltoon_ltsoover    = ("_lil/[Optional] lilToonOverlayOnePass");
        private string liltoon_ltslover    = ("_lil/[Optional] lilToonLiteOverlay");
        private string liltoon_ltsloover   = ("_lil/[Optional] lilToonLiteOverlayOnePass");

        // private string liltoon_ltsbaker    = ("Hidden/ltsother_baker");
        // private string liltoon_ltspo       = ("Hidden/ltspass_opaque");
        // private string liltoon_ltspc       = ("Hidden/ltspass_cutout");
        // private string liltoon_ltspt       = ("Hidden/ltspass_transparent");
        // private string liltoon_ltsptesso   = ("Hidden/ltspass_tess_opaque");
        // private string liltoon_ltsptessc   = ("Hidden/ltspass_tess_cutout");
        // private string liltoon_ltsptesst   = ("Hidden/ltspass_tess_transparent");

        private string liltoon_ltsm        = ("_lil/lilToonMulti");
        private string liltoon_ltsmo       = ("Hidden/lilToonMultiOutline");
        private string liltoon_ltsmref     = ("Hidden/lilToonMultiRefraction");
        private string liltoon_ltsmfur     = ("Hidden/lilToonMultiFur");
        private string liltoon_ltsmgem     = ("Hidden/lilToonMultiGem");
        
        // private string liltoon_mtoon       = ("VRM/MToon");

        private string motchiri_lts        = ("motchiri" + "/lilToon");
        private string motchiri_ltsc       = ("Hidden/" + "motchiri" + "/Cutout");
        private string motchiri_ltst       = ("Hidden/" + "motchiri" + "/Transparent");
        private string motchiri_ltsot      = ("Hidden/" + "motchiri" + "/OnePassTransparent");
        private string motchiri_ltstt      = ("Hidden/" + "motchiri" + "/TwoPassTransparent");

        private string motchiri_ltso       = ("Hidden/" + "motchiri" + "/OpaqueOutline");
        private string motchiri_ltsco      = ("Hidden/" + "motchiri" + "/CutoutOutline");
        private string motchiri_ltsto      = ("Hidden/" + "motchiri" + "/TransparentOutline");
        private string motchiri_ltsoto     = ("Hidden/" + "motchiri" + "/OnePassTransparentOutline");
        private string motchiri_ltstto     = ("Hidden/" + "motchiri" + "/TwoPassTransparentOutline");

        private string motchiri_ltsoo      = ("motchiri" + "/[Optional] OutlineOnly/Opaque");
        private string motchiri_ltscoo     = ("motchiri" + "/[Optional] OutlineOnly/Cutout");
        private string motchiri_ltstoo     = ("motchiri" + "/[Optional] OutlineOnly/Transparent");

        private string motchiri_ltstess    = ("Hidden/" + "motchiri" + "/Tessellation/Opaque");
        private string motchiri_ltstessc   = ("Hidden/" + "motchiri" + "/Tessellation/Cutout");
        private string motchiri_ltstesst   = ("Hidden/" + "motchiri" + "/Tessellation/Transparent");
        private string motchiri_ltstessot  = ("Hidden/" + "motchiri" + "/Tessellation/OnePassTransparent");
        private string motchiri_ltstesstt  = ("Hidden/" + "motchiri" + "/Tessellation/TwoPassTransparent");

        private string motchiri_ltstesso   = ("Hidden/" + "motchiri" + "/Tessellation/OpaqueOutline");
        private string motchiri_ltstessco  = ("Hidden/" + "motchiri" + "/Tessellation/CutoutOutline");
        private string motchiri_ltstessto  = ("Hidden/" + "motchiri" + "/Tessellation/TransparentOutline");
        private string motchiri_ltstessoto = ("Hidden/" + "motchiri" + "/Tessellation/OnePassTransparentOutline");
        private string motchiri_ltstesstto = ("Hidden/" + "motchiri" + "/Tessellation/TwoPassTransparentOutline");

        private string motchiri_ltsl       = ("motchiri" + "/lilToonLite");
        private string motchiri_ltslc      = ("Hidden/" + "motchiri" + "/Lite/Cutout");
        private string motchiri_ltslt      = ("Hidden/" + "motchiri" + "/Lite/Transparent");
        private string motchiri_ltslot     = ("Hidden/" + "motchiri" + "/Lite/OnePassTransparent");
        private string motchiri_ltsltt     = ("Hidden/" + "motchiri" + "/Lite/TwoPassTransparent");

        private string motchiri_ltslo      = ("Hidden/" + "motchiri" + "/Lite/OpaqueOutline");
        private string motchiri_ltslco     = ("Hidden/" + "motchiri" + "/Lite/CutoutOutline");
        private string motchiri_ltslto     = ("Hidden/" + "motchiri" + "/Lite/TransparentOutline");
        private string motchiri_ltsloto    = ("Hidden/" + "motchiri" + "/Lite/OnePassTransparentOutline");
        private string motchiri_ltsltto    = ("Hidden/" + "motchiri" + "/Lite/TwoPassTransparentOutline");

        private string motchiri_ltsref     = ("Hidden/" + "motchiri" + "/Refraction");
        private string motchiri_ltsrefb    = ("Hidden/" + "motchiri" + "/RefractionBlur");
        private string motchiri_ltsfur     = ("Hidden/" + "motchiri" + "/Fur");
        private string motchiri_ltsfurc    = ("Hidden/" + "motchiri" + "/FurCutout");
        private string motchiri_ltsfurtwo  = ("Hidden/" + "motchiri" + "/FurTwoPass");
        private string motchiri_ltsfuro    = ("motchiri" + "/[Optional] FurOnly/Transparent");
        private string motchiri_ltsfuroc   = ("motchiri" + "/[Optional] FurOnly/Cutout");
        private string motchiri_ltsfurotwo = ("motchiri" + "/[Optional] FurOnly/TwoPass");
        
        private string motchiri_ltsgem     = ("Hidden/" + "motchiri" + "/Gem");
        
        private string motchiri_ltsfs      = ("motchiri" + "/[Optional] FakeShadow");

        private string motchiri_ltsover    = ("motchiri" + "/[Optional] Overlay");
        private string motchiri_ltsoover   = ("motchiri" + "/[Optional] OverlayOnePass");
        private string motchiri_ltslover   = ("motchiri" + "/[Optional] LiteOverlay");
        private string motchiri_ltsloover  = ("motchiri" + "/[Optional] LiteOverlayOnePass");

        private string motchiri_ltsm       = ("motchiri" + "/lilToonMulti");
        private string motchiri_ltsmo      = ("Hidden/" + "motchiri" + "/MultiOutline");
        private string motchiri_ltsmref    = ("Hidden/" + "motchiri" + "/MultiRefraction");
        private string motchiri_ltsmfur    = ("Hidden/" + "motchiri" + "/MultiFur");
        private string motchiri_ltsmgem    = ("Hidden/" + "motchiri" + "/MultiGem");
    }
}