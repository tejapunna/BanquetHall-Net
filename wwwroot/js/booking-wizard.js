// New Lead Wizard - Client-side JavaScript (3 steps)
$(document).ready(function () {
    let currentStep = 1;
    let guestId = null;
    let functionId = null;
    let guestData = {};
    let functionData = {};

    // Load dropdown data on page load
    loadFunctionNames();
    loadFunctionHalls();
    loadManagers();

    function loadFunctionNames() {
        $.getJSON('/Manager/Booking/Wizard?handler=FunctionNames', function (data) {
            var select = $('#functionNameId');
            select.empty().append('<option value="">-- Select --</option>');
            data.forEach(function (item) {
                select.append('<option value="' + item.id + '">' + item.name + '</option>');
            });
        });
    }

    function loadFunctionHalls() {
        $.getJSON('/Manager/Booking/Wizard?handler=FunctionHalls', function (data) {
            var select = $('#functionHallId');
            select.empty().append('<option value="">-- Select --</option>');
            data.forEach(function (item) {
                select.append('<option value="' + item.id + '">' + item.name + '</option>');
            });
        });
    }

    function loadManagers() {
        $.getJSON('/Manager/Booking/Wizard?handler=Managers', function (data) {
            var select = $('#assignedManagerId');
            select.empty().append('<option value="">-- Select --</option>');
            data.forEach(function (item) {
                select.append('<option value="' + item.id + '">' + item.fullName + '</option>');
            });
        });
    }

    // Step navigation helpers
    function showStep(step) {
        $('.wizard-step').hide();
        $('#step' + step).show();
        currentStep = step;
        updateStepIndicator(step);
    }

    function updateStepIndicator(step) {
        for (let i = 1; i <= 3; i++) {
            const badge = $('.step-badge[data-step="' + i + '"]');
            const label = $('.step-label[data-step="' + i + '"]');
            const connector = $('.step-connector[data-after="' + i + '"]');

            badge.removeClass('bg-primary bg-success bg-secondary');
            label.removeClass('fw-bold');

            if (i === step) {
                badge.addClass('bg-primary');
                label.addClass('fw-bold');
            } else if (i < step) {
                badge.addClass('bg-success');
                if (connector.length) {
                    connector.removeClass('border-secondary').addClass('border-success');
                }
            } else {
                badge.addClass('bg-secondary');
                if (connector.length) {
                    connector.removeClass('border-success').addClass('border-secondary');
                }
            }
        }
    }

    function clearValidationErrors(form) {
        $(form).find('.is-invalid').removeClass('is-invalid');
        $(form).find('.invalid-feedback').text('');
    }

    function showValidationErrors(form, errors) {
        clearValidationErrors(form);
        if (errors) {
            Object.keys(errors).forEach(function (key) {
                const input = $(form).find('[name="' + key + '"]');
                if (input.length) {
                    input.addClass('is-invalid');
                    input.siblings('.invalid-feedback').text(errors[key].join(', '));
                }
            });
            if (errors.General) {
                alert(errors.General.join('\n'));
            }
        }
    }

    function getAntiForgeryToken() {
        return $('input[name="__RequestVerificationToken"]').val();
    }

    // Smart Autofill Integration
    $(document).on('guestSelected', function (e, guest) {
        if (guest && guest.id) {
            guestId = guest.id;
            guestData = { firstName: guest.name, lastName: '', mobile: guest.mobile, email: guest.email };
            showStep(2);
        }
    });

    $(document).on('click', '#guestSearchResults .list-group-item', function () {
        var selectedGuestId = $(this).data('guest-id');
        if (selectedGuestId) {
            guestId = selectedGuestId;
            showStep(2);
        }
    });

    // Back button navigation
    $(document).on('click', '.btn-back', function () {
        var targetStep = parseInt($(this).data('target'));
        showStep(targetStep);
    });

    // Step 1: Guest Form Submission
    $('#guestForm').on('submit', function (e) {
        e.preventDefault();
        clearValidationErrors(this);

        var data = {
            firstName: $('#guestFirstName').val(),
            lastName: $('#guestLastName').val(),
            mobile: $('#guestMobile').val(),
            email: $('#guestEmail').val() || null,
            village: $('#guestVillage').val() || null,
            mandal: $('#guestMandal').val() || null,
            referredByName: $('#guestReferredByName').val() || null,
            referredByPhone: $('#guestReferredByPhone').val() || null,
            guestAadhaar: $('#guestAadhaar').val() || null,
            guestPan: $('#guestPan').val() || null,
            remarks: $('#guestRemarks').val() || null
        };

        $.ajax({
            url: '/Manager/Booking/Wizard?handler=Step1',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            },
            success: function (response) {
                if (response.success) {
                    guestId = response.data.guestId;
                    guestData = data;
                    showStep(2);
                } else {
                    showValidationErrors('#guestForm', response.errors);
                }
            },
            error: function () {
                alert('An error occurred. Please try again.');
            }
        });
    });

    // Step 2: Function Form Submission
    $('#functionForm').on('submit', function (e) {
        e.preventDefault();
        clearValidationErrors(this);

        var mealType = $('input[name="MealType"]:checked').val() || '';

        var data = {
            guestId: guestId,
            functionDate: $('#functionDate').val(),
            functionNameId: parseInt($('#functionNameId').val()) || 0,
            mealType: mealType,
            mealPlan: $('#mealPlan').val(),
            noOfPacks: parseInt($('#noOfPacks').val()) || 0,
            guaranteedPacks: parseInt($('#guaranteedPacks').val()) || 0,
            specialInstruction: $('#specialInstruction').val() || null,
            assignedManagerId: parseInt($('#assignedManagerId').val()) || 0,
            functionHallId: parseInt($('#functionHallId').val()) || 0
        };

        $.ajax({
            url: '/Manager/Booking/Wizard?handler=Step2',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            },
            success: function (response) {
                if (response.success) {
                    functionId = response.data.functionId;
                    functionData = data;
                    showSummary();
                } else {
                    showValidationErrors('#functionForm', response.errors);
                }
            },
            error: function () {
                alert('An error occurred. Please try again.');
            }
        });
    });

    // Show summary (Step 3)
    function showSummary() {
        showStep(3);

        var functionNameText = $('#functionNameId option:selected').text();
        var functionHallText = $('#functionHallId option:selected').text();
        var managerText = $('#assignedManagerId option:selected').text();

        var html = '<h6 class="mb-3">Guest Information</h6>';
        html += '<table class="table table-bordered table-sm">';
        html += '<tr><th style="width:35%">Name</th><td>' + escHtml(guestData.firstName + ' ' + guestData.lastName) + '</td></tr>';
        html += '<tr><th>Mobile</th><td>' + escHtml(guestData.mobile) + '</td></tr>';
        if (guestData.email) html += '<tr><th>Email</th><td>' + escHtml(guestData.email) + '</td></tr>';
        if (guestData.village) html += '<tr><th>Village</th><td>' + escHtml(guestData.village) + '</td></tr>';
        if (guestData.mandal) html += '<tr><th>Mandal</th><td>' + escHtml(guestData.mandal) + '</td></tr>';
        if (guestData.referredByName) html += '<tr><th>Referred By</th><td>' + escHtml(guestData.referredByName) + (guestData.referredByPhone ? ' (' + escHtml(guestData.referredByPhone) + ')' : '') + '</td></tr>';
        if (guestData.guestAadhaar) html += '<tr><th>Aadhaar</th><td>' + escHtml(guestData.guestAadhaar) + '</td></tr>';
        if (guestData.guestPan) html += '<tr><th>PAN</th><td>' + escHtml(guestData.guestPan) + '</td></tr>';
        if (guestData.remarks) html += '<tr><th>Remarks</th><td>' + escHtml(guestData.remarks) + '</td></tr>';
        html += '</table>';

        html += '<h6 class="mb-3 mt-4">Function Details</h6>';
        html += '<table class="table table-bordered table-sm">';
        html += '<tr><th style="width:35%">Function Date</th><td>' + escHtml(functionData.functionDate) + '</td></tr>';
        html += '<tr><th>Function Name</th><td>' + escHtml(functionNameText) + '</td></tr>';
        html += '<tr><th>Meal Type</th><td>' + escHtml(functionData.mealType) + '</td></tr>';
        html += '<tr><th>Meal Plan</th><td>' + escHtml(functionData.mealPlan) + '</td></tr>';
        html += '<tr><th>No. of Packs</th><td>' + functionData.noOfPacks + '</td></tr>';
        html += '<tr><th>Guaranteed Packs</th><td>' + functionData.guaranteedPacks + '</td></tr>';
        html += '<tr><th>Function Hall</th><td>' + escHtml(functionHallText) + '</td></tr>';
        html += '<tr><th>Assigned Manager</th><td>' + escHtml(managerText) + '</td></tr>';
        if (functionData.specialInstruction) html += '<tr><th>Special Instructions</th><td>' + escHtml(functionData.specialInstruction) + '</td></tr>';
        html += '</table>';

        $('#leadSummary').html(html);
    }

    // Print summary
    $('#btnPrintSummary').on('click', function () {
        var printContent = $('#leadSummary').html();
        var printWindow = window.open('', '_blank');
        printWindow.document.write('<html><head><title>Lead Summary</title>');
        printWindow.document.write('<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">');
        printWindow.document.write('</head><body class="p-4">');
        printWindow.document.write('<h4 class="mb-3">Lead Summary</h4>');
        printWindow.document.write(printContent);
        printWindow.document.write('</body></html>');
        printWindow.document.close();
        printWindow.onload = function () { printWindow.print(); };
    });

    function escHtml(str) {
        if (!str) return '';
        return $('<div>').text(str).html();
    }

    // Initialize
    showStep(1);
});
