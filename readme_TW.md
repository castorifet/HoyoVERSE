# HoyoVERSE

一個米遊遊戲（不止）啟動器

## 功能
* 啟動/安裝/更新米遊
* 加載官方Banners/抽卡紀錄查詢
* 多語言支援
* 無縫切換CN/OS伺服器
* 安裝遊戲自定義目錄至
* 其他遊戲/程式添加
* 功能為Unity製作之遊戲
* FPS 修改器 (Star Rail only)

## 編譯方法

下載 & 安裝 Visual Studio

安裝.NET 開發環境

打開你的專案，然後編譯。

## 問題已知

切換至CN問題發生可能

抽卡紀錄開啟失敗可能 / 崩潰可能（未安裝遊戲）

Switching Language may cause ArgumentOutOfRange Exception error

## 系統要求

Microsoft Windows 10 and newer

x64 位元 (arm64 附帶x64翻譯器, 21277+)

網際網路連線穩定

## 問題有?

issue 開啟一個

## FAQ

Q: The launcher can not be launched.

A: Path can not contain any non-ASCII characters. Go and check your game path and launcher path.

Q: DLL Not found. 

A: Ensure that ALL the DLLs exists n your launcher directory. If the problem persists, check your .NET runtime installation.

Q: Game cannot be downloaded.

A: Check your internet connection. If your internet is fine, then go and check HoYolab or whatever forums. (this is Hoyo's fault)

Q: Game crashes after launching.

A: Same as 'The launcher can not be launched.'

Q: Game is not launching

A: Check your directory permission, and check whether the game did really exist.


I have added some comments inside the source since it may be too complicated for noobs to understand the codes.

### Yes, It's u, @XingChenBanYue (
