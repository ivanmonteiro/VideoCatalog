var currentImg = null;
var previewAll = false;

jQuery(document).ready(function () {
    jQuery(".video-thumb-container").mouseenter(function () {
        if (!previewAll) {
            currentImg = jQuery(this).find("img");
            //cycleImage(currentImg);
        } else {
            currentImg = null;
        }
    });

    jQuery(".video-thumb-container").mouseleave(function () {
        currentImg = null;
    });

    jQuery("a.video-thumb-link").click(function () {
        var path = jQuery(this).data("file");
        jQuery.post("/Home/OpenVideo", { path: path }, function (data) {
            //empty response, just waiting the file to be opened
        });
    });

    jQuery("a.preview-all-link").click(function () {
        currentImg = null;
        if (previewAll) {
            jQuery(this).text("Preview All");
            previewAll = false;
        } else {
            jQuery(this).text("Stop");
            previewAll = true;
        }
    });
    
    setInterval(cycleAll, 2000);
    setInterval(nextFrame, 700);
});

function cycleAll() {
    if (previewAll) {
        jQuery(".video-thumb-container").each(function (index) {
            cycleImage(jQuery(this).find("img"));
        });
    }
}

function nextFrame() {
    if (currentImg != null) {
        cycleImage(currentImg);
        //setTimeout(nextFrame, 500);
    }
}

function cycleImage(img) {
    var base_path = img.data("base-path");
    var nextThumbIndex = parseInt(img.data("thumb-index")) + 1;
    if (nextThumbIndex > 7) {
        nextThumbIndex = 0;
    }
    img.data("thumb-index", nextThumbIndex);
    var image_path = "/Home/GetImage?path=" + base_path + "_" + nextThumbIndex;
    img.attr("src", image_path);
}