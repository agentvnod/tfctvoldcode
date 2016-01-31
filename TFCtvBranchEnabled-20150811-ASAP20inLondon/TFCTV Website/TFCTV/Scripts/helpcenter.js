/* File Created: March 6, 2012 */

function clearHelpCenter() {
    $('.catQuestionsList').empty();
    $('.catQuestionsMenu > h1').empty();
    $('.subcategoryMenu').empty();
}
function getSubCats(id) {
    var ctr = 0;
    clearHelpCenter();
    $.getJSON("/Help/GetSubCategories", { id: id }, function (data) {
        if (data.length > 0) {
            $.each(data, function (x, y) {
                if (ctr == 0) {
                    $('.catQuestionsMenu > h1').html(y.description);
                    getQuestions(y.id);
                }
                var li = '<li class="scat" rel="' + y.id + '"><a href="#">' + y.description + '</a></li>';
                $('.subcategoryMenu').append(li);
                ctr++;
            });
        }
    });
}

function getQuestions(id) {
    $.getJSON("/Help/GetQuestions", { id: id }, function (data) {
        $('.catQuestionsList').empty();
        if (data.length > 0) {
            $.each(data, function (x, y) {
                var li = '<li class="question" rel="' + y.id + '"><a href="/Help/Question/' + y.id + '">' + y.Q + '</a>';
                //li += '<div class="questionContent hideElement">';
                //li += '<p>' + decodeURIComponent(y.Answer) + '</p>';
                //li += '<ul class="relatedquestions">';
                //li += '<li><a href="#">Lorem ipsum dolor sit amet, consectetur viverra quis nec risus? </a></li>';
                //                li += '<li><a href="#">Lorem ipsum dolor sit amet, consectetur adipiscing?</a></li>';
                //                li += '<li><a href="#">Donec tincidunt ipsum sit amet ipsum consectetur viverra quis nec risus? </a></li>';
                //                li += '<li><a href="#">Amet ipsum consectetur viverra quis nec risus? </a></li>';
                //li += '</ul></div>';
                li += '</li>';
                $('.catQuestionsList').append(li);
            });
        }
    });
}