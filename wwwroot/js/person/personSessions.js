jQuery(document).ready(function(){
    setTimeout( function(){
        $("#personSessionsWrapper").load(`/People/${currentPersonID}/sessions`);
        $("#personProceduresWrapper").load(`/People/${currentPersonID}/procedures`);
        $("#personDocumentsWrapper").load(`/People/${currentPersonID}/documents`);
        $("#personRecoveryAccountWrapper").load(`/People/${currentPersonID}/recovery-requests`);
    }, 500);
});