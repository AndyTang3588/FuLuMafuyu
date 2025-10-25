using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;


namespace RnwNadesystem
{
    public static class Localize
    {
        public static string[] avatar = new string[]{"Avatar", "アバター", "화신"};
        public static string[] contactParameters = new string[]{"Contact Parameters", "コンタクトパラメーター", "콘택트 매개변수"};
        public static string[] contactRadius = new string[]{"Contact Radius", "コンタクトの半径", "콘택트 반경"};
        public static string[] contactOffsetY = new string[]{"Contact Offset Y", "コンタクトのYオフセット", "콘택트 Y 오프셋"};
        public static string[] shadowShaderInstall = new string[]{"Shadow Shader Install", "影シェーダーの導入", "그림자 셰이더 도입"};
        public static string[] installHands = new string[]{"Install Hands", "手へ導入(撫でる用)", "손에 도입"};
        public static string[] installHead = new string[]{"Install Head", "頭へ導入(撫でられる用)", "머리에 도입"};

        public static string[] notSelectAvatarTitle = new string[]{
            "Nade System Install Error",
            "撫でギミック導入エラー",
            "네이드 시스템 설치 오류"
        };
        public static string[] notSelectAvatarMsg = new string[]{
            "Select avatar and then press the Setup button.",
            "アバターを選択してからセットアップボタンを押してください。",
            "아바타를 선택한 후 설정 버튼을 누릅니다."
        };

    }
}
