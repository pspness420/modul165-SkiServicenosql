async function book() {
    const refreshToken = localStorage.getItem("refreshToken");

    const priority = document.getElementById("priority").value;
    const isSmallService = document.getElementById("smallService").checked;
    const isLargeService = document.getElementById("largeService").checked;

    serviceType = isSmallService ? "small" : isLargeService ? "large" : null;

    if (!serviceType) {
        alert("Bitte wählen Sie eine Serviceart aus.");
        return;
    }

    const hasRaceService = document.getElementById("raceService").checked;
    const hasBindingService = document.getElementById("bindingService").checked;
    const hasSkinService = document.getElementById("skinService").checked;
    const hasWaxService = document.getElementById("waxService").checked;

    let result = await fetch("/api/auftrag/book", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ priority, serviceType, hasRaceService, hasBindingService, hasSkinService, hasWaxService, refreshToken })
    });

    if (result.status == 401) {
        alert("Bitte melden Sie sich an.");
        window.location.href = "login.html";
        return;
    }

    if (!result.ok) {
        alert("Fehler beim Buchen.");
        return;
    }
}

async function getBookings() {
    const refreshToken = localStorage.getItem("refreshToken");

    let result = await fetch("/api/auftrag/bookings", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ refreshToken })
    });

    if (result.status == 401) {
        window.location.href = "login.html";
        return;
    }

    if (!result.ok) {
        alert("Fehler beim Abrufen der Buchungen.");
        return;
    }

    let bookings = await result.json();

    let table = document.getElementById("bookingsTable");
    table.innerHTML = "<tr><th>Pickup Date</th><th> STATUS </th><th> Dienstleistungen </th></tr>";

    bookings.forEach(booking => {
        let row = table.insertRow();
        row.insertCell().textContent = booking.pickupDate;
        row.insertCell().textContent = booking.status;
        row.insertCell().textContent = (booking.hasRaceService ? "✅" : "❌") + "Rennski-Service";
        row.insertCell().textContent = (booking.hasBindingService ? "✅" : "❌") + "Bindung montieren und einstellen";
        row.insertCell().textContent = (booking.hasSkinService ? "✅" : "❌") + "Fell zuschneiden";
        row.insertCell().textContent = (booking.hasWaxService ? "✅" : "❌") + "Heißwachsen";
    });
}

document.getElementById("registrationForm").addEventListener("submit", async function (event) {
    event.preventDefault();
    await book();
    await getBookings();
});

getBookings();

