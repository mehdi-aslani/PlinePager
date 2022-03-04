// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.querySelectorAll(".mvc-grid").forEach(element => new MvcGrid(element));

MvcGrid.lang = {
    text: {
        'contains': 'شامل',
        'equals': 'مساوی با',
        'not-equals': 'مساوی نباشد با',
        'starts-with': 'شروع با',
        'ends-with': 'اتمام با'
    },
    number: {
        'equals': 'مساوی با',
        'not-equals': 'مساوی نباشد با',
        'less-than': 'کوچکتر از',
        'greater-than': 'بزرگتر از',
        'less-than-or-equal': 'کوچکتر و مساوی با',
        'greater-than-o-requal': 'بزرگتر و مساوی با'
    },
    date: {
        'equals': 'مساوی با',
        'not-equals': 'مساوی نباشد با',
        'earlier-than': 'قبل از',
        'later-than': 'بعد از',
        'earlier-than-or-equal': 'قبل و مساوی با',
        'later-than-or-equal': 'بعد و مساوی با'
    },
    enum: {
        'equals': 'مساوی با',
        'not-equals': 'مساوی نباشد با'
    },
    boolean: {
        'equals': 'مساوی با',
        'not-equals': 'مساوی نباشد با'
    },
    guid: {
        'equals': 'مساوی با',
        'not-equals': 'مساوی نباشد با',
    },
    filter: {
        'apply': '✔',
        'remove': '✘'
    },
    operator: {
        'select': '',
        'and': 'و',
        'or': 'یا'
    }
};

