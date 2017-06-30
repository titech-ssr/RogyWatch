        // 接続先URI
        //var uri = "ws://" + location.host + "/sample/chat";
				var uri = "ws://172.16.16.4:8000/";
				//var uri = "ws://localhost:8000/";

        // WebSocketオブジェクト
        var webSocket = null;

        // 初期処理
        function init() {
            // ボタン押下イベント設定
            $("[data-name='message']").keypress(press);
            // 接続
            open();
        }

        // 接続
        function open() {
            if (webSocket == null) {
                // WebSocket の初期化
                webSocket = new WebSocket(uri);
                // イベントハンドラの設定
                webSocket.onopen = onOpen;
                webSocket.onmessage = onMessage;
                webSocket.onclose = onClose;
                webSocket.onerror = onError;
            }
        }

        // 接続イベント
        function onOpen(event) {
            chat("接続しました");
						var err = new WebSocket('ws://172.16.16.4:8500');
						err.onmessage = (e)=>{
							console.log(e.data);
							//chat(e.data);
						};
        }

        // メッセージ受信イベント
        function onMessage(event) {
            if (event && event.data) {
                chat(event.data);
            }
        }

        // エラーイベント
        function onError(event) {
            //chat("エラーが発生しました");
        }

        // 切断イベント
        function onClose(event) {
            chat("切断しました3秒後に再接続します(" + event.code + ")");
            webSocket = null;
            setTimeout("open()", 3000);
        }

        // キー押下時
        function press(event) {
            // キーがEnterか判定
            if (event && event.which == 13) {
                // メッセージ取得
                var message = $("[data-name='message']").val();
                // 存在チェック
                if (message && webSocket) {
                    // メッセージ送信
                    webSocket.send("" + message);
                    // メッセージ初期化
                    $("[data-name='message']").val("");
                }
            }
        }

        // チャットに表示
        function chat1(message) {
            // 100件まで残す
            var chats = $("[data-name='chat1']").find("div");
            while (chats.length >= 200) {
                chats = chats.last().remove();
            }
            // メッセージ表示
            var msgtag = $("<div>").text(message);
            $("[data-name='chat1']").prepend(msgtag);
        }
        function chat2(message) {
            // 200件まで残す
            var chats = $("[data-name='chat2']").find("div");
            while (chats.length >= 200) {
                chats = chats.last().remove();
            }
            // メッセージ表示
            var msgtag = $("<div>").text(message);
            $("[data-name='chat2']").prepend(msgtag);
        }

        // 初期処理登録

      
        var std = null;
        var err = null;
        $("#stdopen").click(function stdopen(){
          std = new WebSocket("ws://172.16.16.4:8000");
          std.onopen = onOpen;
          std.onmessage = onMessage;
          std.onclose = onClose;
          std.onerror = onError;

          function onOpen(event) {
            chat1("Connected");
          }

          function onMessage(event) {
            if (event && event.data) {
                chat1(event.data);
            }
          }
          function onClose(event) {
            chat1("disconnected");
            std = null;
          }
          function onError(event) {
            console.log(event);
          }


        });

        $("#erropen").click(function stdopen(){
          err = new WebSocket('ws://172.16.16.4:8500');
          console.log(err);
          err.onopen = onOpen;
          err.onmessage = onMessage;
          err.onclose = onClose;
          err.onerror = onError;

          function onOpen(event) {
            chat2("Connected");
          }

          function onMessage(event) {
            if (event && event.data) {
                chat2(event.data);
            }
          }
          function onClose(event) {
            chat2("disconnected");
            err = null;
          }
          function onError(event) {
            console.log(event);
          }

        });

        var stdin = document.getElementById("stdin");
        $("#stdin").keydown((e)=>{
          if (e.keyCode != 13) return;

          //(std||err).send(stdin.value);
          std.send(stdin.value);
          stdin.value = "";
        });



