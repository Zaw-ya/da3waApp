// Navbar scroll effect
$(window).scroll(function () {
    if ($(window).scrollTop() > 50) {
        $('.navbar').css({
            'backgroundColor': 'rgba(255, 255, 255, 0.98)',
            'boxShadow': '0 5px 20px rgba(0, 0, 0, 0.1)'
        });
    } else {
        $('.navbar').css({
            'backgroundColor': 'rgba(255, 255, 255, 0.98)',
            'boxShadow': '0 2px 15px rgba(0, 0, 0, 0.08)'
        });
    }
});

// Smooth scrolling for anchor links
$(document).on('click', 'a[href^="#"]', function (e) {
    var target = $(this.getAttribute('href'));
    if (target.length) {
        e.preventDefault();
        $('html, body').stop().animate({
            scrollTop: target.offset().top - 80
        }, 1000);
    }
});

// Update active nav link on scroll
$(window).on('scroll', function () {
    var scrollPos = $(document).scrollTop() + 100;
    $('a.nav-link').each(function () {
        var currLink = $(this);
        var refElement = $(currLink.attr("href"));
        if (refElement.length && refElement.position().top <= scrollPos && refElement.position().top + refElement.height() > scrollPos) {
            $('a.nav-link').removeClass("active");
            currLink.addClass("active");
        }
    });
});

// Form submission handler
$(document).ready(function () {
    $('#contactForm').on('submit', function (e) {
        e.preventDefault();

        var name = $('#name').val();
        var email = $('#email').val();
        var subject = $('#subject').val();
        var message = $('#message').val();

        if (name && email && subject && message) {
            alert('Thank you for contacting us! We will get back to you soon.');
            $('#contactForm')[0].reset();
        } else {
            alert('Please fill in all required fields.');
        }
    });
});


