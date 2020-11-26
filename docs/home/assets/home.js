!(function($) {
  "use strict";

  // Smooth scroll for the navigation menu and links with .scrollto classes
  $(document).on('click', '.scrollto', function(e) {
    if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
      e.preventDefault();
      var target = $(this.hash);
      if (target.length) {

        var scrollto = target.offset().top;
        scrollto = scrollto

        if ($(this).attr("href") == '#header') {
          scrollto = 0;
        }

        $('html, body').animate({
          scrollTop: scrollto
        }, 1500, 'easeInOutExpo');

        return false;
      }
    }
  });

  // Activate smooth scroll on page load with hash links in the url
  $(document).ready(function() {
    if (window.location.hash) {
      var initial_nav = window.location.hash;
      if ($(initial_nav).length) {
        var scrollto = $(initial_nav).offset().top;
        scrollto = scrollto
        $('html, body').animate({
          scrollTop: scrollto
        }, 1500, 'easeInOutExpo');
      }
    }
  });


  // Feature panels linking
  $('div[feature]#filter').on('click', () => navigateTo('usage/reading/filtering.html'));
  $('div[feature]#sort').on('click', () => navigateTo('usage/reading/sorting.html'));
  $('div[feature]#pagination').on('click', () => navigateTo('usage/reading/pagination.html'));
  $('div[feature]#selection').on('click', () => navigateTo('usage/reading/sparse-fieldset-selection.html'));
  $('div[feature]#include').on('click', () => navigateTo('usage/reading/including-relationships.html'));
  $('div[feature]#security').on('click', () => navigateTo('usage/resources/attributes.html#capabilities'));
  $('div[feature]#validation').on('click', () => navigateTo('usage/options.html#enable-modelstate-validation'));
  $('div[feature]#customizable').on('click', () => navigateTo('usage/resources/resource-definitions.html'));


  const navigateTo = (url) => {
    if (!window.getSelection().toString()){
      window.location = url;
    }
  }


  hljs.initHighlightingOnLoad()

  // Back to top button
  $(window).scroll(function() {
    if ($(this).scrollTop() > 100) {
      $('.back-to-top').fadeIn('slow');
    } else {
      $('.back-to-top').fadeOut('slow');
    }
  });

  $('.back-to-top').click(function() {
    $('html, body').animate({
      scrollTop: 0
    }, 1500, 'easeInOutExpo');
    return false;
  });
  // Init AOS
  function aos_init() {
    AOS.init({
      duration: 800,
      easing: "ease-in-out",
      once: true
    });
  }
  $(window).on('load', function() {
    aos_init();
  });

})(jQuery);