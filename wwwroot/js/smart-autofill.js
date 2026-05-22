// Smart Autofill - Guest Search with debounce
$(document).ready(function () {
    var searchTimeout = null;

    $('#guestSearchInput').on('input', function () {
        var term = $(this).val();
        clearTimeout(searchTimeout);

        if (term.length < 3) {
            $('#guestSearchResults').hide().empty();
            return;
        }

        searchTimeout = setTimeout(function () {
            $.ajax({
                url: '/Api/GuestSearch?handler=Search&term=' + encodeURIComponent(term),
                type: 'GET',
                success: function (results) {
                    var container = $('#guestSearchResults');
                    container.empty();

                    if (results.length === 0) {
                        container.append('<div class="list-group-item text-muted">No matching guests found</div>');
                    } else {
                        results.forEach(function (guest) {
                            var item = $('<a href="#" class="list-group-item list-group-item-action"></a>');
                            item.data('guest-id', guest.id);
                            var text = guest.name + ' - ' + guest.mobile;
                            if (guest.email) text += ' (' + guest.email + ')';
                            item.text(text);
                            if (guest.status) {
                                item.append(' <span class="badge bg-info">' + $('<span>').text(guest.status).html() + '</span>');
                            }
                            item.on('click', function (e) {
                                e.preventDefault();
                                selectGuest(guest.id);
                            });
                            container.append(item);
                        });
                    }
                    container.show();
                }
            });
        }, 300);
    });

    // Hide results when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#guestSearchInput, #guestSearchResults').length) {
            $('#guestSearchResults').hide();
        }
    });

    function selectGuest(guestId) {
        $.ajax({
            url: '/Api/Autofill?handler=GuestDetail&id=' + guestId,
            type: 'GET',
            success: function (detail) {
                if (detail.error) {
                    alert(detail.error);
                    return;
                }

                // Auto-fill guest form fields
                $('#guestName').val(detail.name);
                $('#guestMobile').val(detail.mobile);
                $('#guestEmail').val(detail.email || '');
                $('#guestReferredByName').val(detail.referredByName || '');
                $('#guestReferredByPhone').val(detail.referredByPhone || '');
                $('#guestStatus').val(detail.status || '');

                // Show function history
                if (detail.functions && detail.functions.length > 0) {
                    var tbody = $('#functionHistoryBody');
                    tbody.empty();
                    detail.functions.forEach(function (fn) {
                        tbody.append('<tr><td>' + new Date(fn.functionDate).toLocaleDateString() + '</td><td>' + fn.functionType + '</td><td>' + fn.mealPlan + '</td></tr>');
                    });
                    $('#guestFunctionHistory').show();
                }

                // Show manager info
                $('#initiatedByManager').text(detail.initiatedByManager || '-');
                $('#currentManager').text(detail.currentFollowUpManager || '-');

                // Hide search results
                $('#guestSearchResults').hide();
                $('#guestSearchInput').val(detail.name);

                // Trigger event for wizard integration
                $(document).trigger('guestSelected', [{ id: guestId }]);
            }
        });
    }
});
