jp,

-注意事項
このパッチファイルは単独で使用することはできず、オリジナルアバターを購入したユーザーのみ使用することができます。
使用したいアバターの最新バージョンをダウンロードして準備してください。
元のモデルFBXデータとプロジェクトのパスに修正が加えられていないプロジェクトに基づいて動作するように設計されています。

オリジナルアバターのバージョン表記が同一であっても、
booth にアップロードされたファイルが更新されるとパッチが動作せずにErrorが発生します。
その場合、最新のアバターファイルをbooth を通じて再度ダウンロードして試していただき、
それでも作動しなければboothのメッセージまたはTwitterのDM等を通じてご連絡お願いします。

基本状態では破綻しないようにされていますが、多くのシェイプキーを利用してカスタマイズする場合は、
メッシュ崩れなどの問題が発生する可能性があります。
動作プログラムVRCFTのバージョン5.0以下では動作しません。

使用規約及びアップロード制限事項などは、オリジナルアバターモデルと同じです。
注意事項及び導入説明の不遵守による誤った適用による被害について責任を負いません。


-導入説明
このパッチファイルは、元のモデルFBXデータ及びプロジェクトのパスに修正が加えられていないプロジェクトを基準に動作するように設計されています。
UnityPackageをプロジェクトにImportした後 
Unity Toolbar MenuでFT Patch - FacialTracking Patchを選択した後、表示されるウィンドウでStartを押してください。

パッチが完了したら、FacialTrackingフォルダ内に用意されたSceneファイルまたはVariant Prefabを使用してください。

顔の基本表情シェイプキーが口元の動きに合わせて基本に戻れるように設定する補正用レイヤーがあります。
MouthDefaultStateのアニメーションファのイルにフェイシャル動作時にデフォルトに戻したいシェイプキーを追加してください。


既存のアバターと同じブループリントを使用してアップデートする場合、
OSC信号がアバターに正しく同期しない場合があります。この場合、Windowsエクスプローラを開いて
C:Users/(WindowsUserName)/AppData/LocalLow/VRChat/VRChat/OSC
該当経路のOSCフォルダ内部の内容を削除した後、もう一度お試しください。


制作基準及びテストデバイス - Vive Pro Eye & FacialTracker , Meta QuestPro
動作OSCプログラム - VRCFT v5.0 
https://github.com/benaclejames/VRCFaceTracking



kor,

-주의사항
본 패치파일은 단독으로 사용할 수 없으며, 원본 아바타를 구매한 사용자만 사용할 수 있습니다.
사용하려는 아바타의 최신 버전을 다운로드하여 준비해 주십시오.
또한 원본 모델 FBX 데이터 및 프로젝트의 경로에 수정이 가해지지 않은
프로젝트를 기준으로 작동하게 설계되어 있습니다.

사용하고 계신 아바타 파일과 버전 표기가 동일하더라도,
booth에 업로드되어있는 파일이 업데이트를 통해 변경되면 패치가 작동하지 않고 Error가 발생합니다.
그럴경우 원본 아바타의 최신 파일을 다시 다운로드하여 적용해 보시고,
그럼에도 작동하지 않을 시 booth의 메시지 또는 Twitter의 DM 등을 통해 연락 부탁드립니다.

기본 상태에서 파탄되지 않게끔 되어있지만, 많은 쉐이프키를 이용하여 커스터마이징 시에는
매쉬 깨짐등의 문제가 발생할 수 있습니다.
작동 프로그램 VRCFT 버전 5.0 미만에서는 동작하지 않습니다.

사용 규약 및 업로드 제한사항 등은 원본 아바타 모델과 동일합니다.
주의사항 및 도입설명 미준수로 잘못된 적용으로 인한 피해에 대해서 책임지지 않습니다.


-도입설명
본 패치파일은 원본 모델 FBX 데이터 및 프로젝트의 경로에 수정이 가해지지 않은 프로젝트를 기준으로 작동하게 설계되어 있습니다.
UnityPackage를 프로젝트에 Import 한 후에 
Unity Toolbar Menu에서 FT Patch - FacialTracking Patch 선택 후 나타나는 창에서 Start 를 눌러주세요.

패치가 완료되면 FacialTracking 폴더 안에 준비된 Scene 파일 또는 Variant Prefab을 사용하시면 됩니다.

얼굴의 기본 표정 쉐이프키가 입가 움직임에 맞춰 기본으로 돌아갈 수 있게 설정하는
보정용 레이어 MouthDefaultCorrection 가 있습니다.
MouthDefault State의 애니메이션 파일에 페이셜 동작시 기본으로 되돌리고 싶은 쉐이프키를 추가해 주시기 바랍니다.

기존 사용하던 아바타와 동일한 Blueprint 를 사용하여 업데이트 할 경우
OSC 신호가 아바타에 제대로 동기화되지 않는 경우가 있습니다. 이런 경우 윈도 탐색기를 열고
C:/Users/(WindowsUserName)/AppData/LocalLow/VRChat/VRChat/OSC
해당 경로 OSC 폴더 내부 항목들을 삭제한 뒤에 다시 시도해 주세요.


제작 기준 및 테스트 기기 - Vive Pro Eye & FacialTracker , Meta QuestPro
작동 OSC 프로그램 - VRCFT v5.0 
https://github.com/benaclejames/VRCFaceTracking



en,

-Please note
This patch file cannot be used by itself and can only be used by users who have purchased the original avatar.
Please download and prepare the latest version of the avatar you wish to use.
It is designed to work based on the original model FBX data and project with no modifications made to the project path.

Even if the avatar file you are using has the same version number,
If the file uploaded to the booth is changed through an update, the patch will not work and an error will occur.
In that case, please download the latest file of the original avatar again and try to apply it,
If it still doesn't work, please contact us via message in booth or DM on Twitter.

The basic state of the avatar is designed not to break down, but if you customize it using many shape keys,
mesh collapse and other problems may occur.
This program does not work with VRCFT version 5.0 or lower.

Terms of use and upload restrictions are the same as for the original avatar model.
We are not responsible for any damage caused by incorrect application due to non-compliance with the precautions and installation instructions.


-Introduction
This patch file is designed to work based on the original model FBX data and the project's paths being unmodified.
After importing the UnityPackage into your project, 
selecting FT Patch - FacialTracking Patch in the Unity Toolbar Menu, press Start in the window that appears.

Once patched, use the Scene file or Variant Prefab prepared inside the FacialTracking folder.

There is a correction layer that sets the basic facial expression shape key to return to basic as the mouth moves.
To use, MouthDefaultState animation fa il and add the shape key you want to revert to default during facial movements.

If updating using the same blueprint as an existing avatar,
OSC signals may not synchronize properly with the avatar. In this case, open Windows Explorer and go to
C:Users/(WindowsUserName)/AppData/LocalLow/VRChat/VRChat/OSC
Delete the contents inside the OSC folder for the relevant route and try again.



Created and Test Devices - Vive Pro Eye & FacialTracker , Meta QuestPro
OSC Program - VRCFT v5.0 
https://github.com/benaclejames/VRCFaceTracking




Special Thanks to...
VRCFT, https://github.com/benaclejames/VRCFaceTracking
Adjerry91, https://github.com/Adjerry91
regzo2's OSCmooth, https://github.com/regzo2/OSCmooth
sisong's HDiffPatch, https://github.com/sisong/HDiffPatch
9-Bone, https://github.com/BonhyeonGu

