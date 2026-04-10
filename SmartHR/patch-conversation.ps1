$p = 'Views/Chat/Conversation.cshtml'
$c = Get-Content $p -Raw

$old = '        <div class="card-footer bg-white border-top p-3">[\s\S]*?<form asp-action="SendMessage" method="post" class="d-flex gap-2">[\s\S]*?<button type="submit" class="btn btn-primary rounded-circle" style="width: 44px; height: 44px; padding: 0;">[\s\S]*?</button>[\s\S]*?</form>[\s\S]*?</div>'

$new = @"
        <div class="card-footer bg-white border-top p-3">
            <form asp-action="SendMessage" method="post" enctype="multipart/form-data" class="d-flex gap-2">
                <input type="hidden" name="receiverId" value="@otherUser?.Id" />
                <button type="button" class="btn btn-outline-secondary rounded-circle" style="width: 44px; height: 44px; padding: 0;" onclick="document.getElementById('attach').click();">
                    <i class="fas fa-paperclip"></i>
                </button>
                <input type="file" id="attach" name="attachment" style="display:none;" onchange="document.getElementById('msgInput').placeholder = this.files[0].name;" />
                <input type="text" id="msgInput" name="content" class="form-control px-4 rounded-pill" placeholder="اكتب رسالتك هنا..." required autocomplete="off" />
                <button type="submit" class="btn btn-primary rounded-circle" style="width: 44px; height: 44px; padding: 0;">
                    <i class="fas fa-paper-plane"></i>
                </button>
            </form>
        </div>
