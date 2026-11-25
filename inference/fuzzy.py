from __future__ import annotations

AMAN, RENDAH, SEDANG, TINGGI = "AMAN", "RENDAH", "SEDANG", "TINGGI"
TIDAK_ADA_DATA = "TIDAK ADA DATA"
PROFIL = {AMAN: 0, RENDAH: 1, SEDANG: 2, TINGGI: 3}


def trap(x, a, b, c, d):
    if x < a or x > d:
        return 0.0
    if b == a:
        if x <= c:
            return 1.0
    elif x < b:
        return (x - a) / (b - a)
    if c == d:
        if x >= b:
            return 1.0
    elif x > c:
        return (d - x) / (d - c)
    return 1.0


def tri(x, a, b, c):
    return trap(x, a, b, b, c)


def sing(x, p):
    return 1.0 if abs(x - p) < 1e-9 else 0.0


RANGE = {
    "ips": (0.0, 4.0),
    "tren_ips": (-4.0, 4.0),
    "kualitas_nilai": (0.0, 10.0),
    "pengulangan": (0.0, 1.0),
    "alfa_mk": (0.0, 100.0),
    "akumulasi_menit": (0.0, 2500.0),
    "jam_gabungan": (0.0, 200.0),
    "status": (0.0, 4.0),
    "sisa_masa_studi": (0.0, 14.0),
}

SETS = {
    "ips": {
        "Kurang": (trap, (0.0, 0.0, 1.8, 2.0)),
        "Cukup": (trap, (1.8, 2.0, 2.8, 3.0)),
        "Baik": (trap, (2.8, 3.0, 4.0, 4.0)),
    },
    "tren_ips": {
        "Turun Drastis": (trap, (-4.0, -4.0, -0.55, -0.5)),
        "Turun Ringan": (trap, (-0.55, -0.45, -0.3, -0.25)),
        "Naik / Stabil": (trap, (-0.3, -0.25, 4.0, 4.0)),
    },
    "kualitas_nilai": {
        "Bersih": (trap, (0.0, 0.0, 0.5, 1.0)),
        "Toleransi": (trap, (0.5, 1.0, 2.0, 2.5)),
        "Tidak Lulus": (trap, (2.0, 3.0, 10.0, 10.0)),
    },
    "pengulangan": {
        "Normal": (sing, (0.0,)),
        "Mengulang": (sing, (1.0,)),
    },
    "alfa_mk": {
        "Sedikit": (trap, (0.0, 0.0, 8.0, 10.0)),
        "Sedang": (trap, (8.0, 10.0, 18.0, 20.0)),
        "Banyak": (trap, (18.0, 20.0, 100.0, 100.0)),
    },
    "akumulasi_menit": {
        "Sedikit": (trap, (0.0, 0.0, 325.0, 375.0)),
        "Sedang": (trap, (325.0, 375.0, 700.0, 750.0)),
        "Banyak (SP I)": (trap, (700.0, 750.0, 1450.0, 1500.0)),
        "Kritis (SP II/III/DO)": (trap, (1450.0, 1500.0, 2500.0, 2500.0)),
    },
    "jam_gabungan": {
        "Sedikit": (trap, (0.0, 0.0, 33.0, 38.0)),
        "Sedang": (trap, (33.0, 38.0, 70.0, 76.0)),
        "Banyak": (trap, (70.0, 76.0, 108.0, 114.0)),
        "Melebihi Batas": (trap, (108.0, 114.0, 200.0, 200.0)),
    },
    "status": {
        "Aktif Normal": (sing, (0.0,)),
        "Cuti Non-Akademik": (sing, (1.0,)),
        "Cuti Akademik": (sing, (2.0,)),
        "Mangkir 1x": (sing, (3.0,)),
        "Mangkir Beruntun": (sing, (4.0,)),
    },
    "sisa_masa_studi": {
        "Habis": (trap, (0.0, 0.0, 0.5, 1.0)),
        "Kritis": (tri, (0.5, 1.0, 2.0)),
        "Menipis": (trap, (1.5, 2.0, 3.0, 4.0)),
        "Longgar": (trap, (3.0, 4.0, 14.0, 14.0)),
    },
}


def fuzzify(var, x):
    lo, hi = RANGE[var]
    x = max(lo, min(hi, float(x)))
    return {name: fn(x, *args) for name, (fn, args) in SETS[var].items()}


def M(m, var, name):
    return m[var][name]


def AND(*xs):
    return min(xs)


def OR(*xs):
    return max(xs)


def NOT(x):
    return 1.0 - x


RULES = {
    "akademik": [
        (
            "R-AKD-1",
            "IPS baik, nilai bersih, tidak mengulang, tren naik/stabil",
            lambda m, v: AND(
                M(m, "ips", "Baik"),
                M(m, "kualitas_nilai", "Bersih"),
                M(m, "pengulangan", "Normal"),
                M(m, "tren_ips", "Naik / Stabil"),
            ),
            10,
        ),
        (
            "R-AKD-2",
            "IPS cukup atau tren turun ringan, nilai bersih, tidak mengulang",
            lambda m, v: AND(
                OR(M(m, "ips", "Cukup"), M(m, "tren_ips", "Turun Ringan")),
                M(m, "kualitas_nilai", "Bersih"),
                M(m, "pengulangan", "Normal"),
            ),
            35,
        ),
        (
            "R-AKD-3",
            "IPS cukup atau tren turun ringan, nilai toleransi, tidak mengulang",
            lambda m, v: AND(
                OR(M(m, "ips", "Cukup"), M(m, "tren_ips", "Turun Ringan")),
                M(m, "kualitas_nilai", "Toleransi"),
                M(m, "pengulangan", "Normal"),
            ),
            35,
        ),
        (
            "R-AKD-4",
            "IPS cukup dan tren turun drastis",
            lambda m, v: AND(M(m, "ips", "Cukup"), M(m, "tren_ips", "Turun Drastis")),
            65,
        ),
        (
            "R-AKD-5",
            "Nilai toleransi dengan D=2",
            lambda m, v: AND(
                M(m, "kualitas_nilai", "Toleransi"),
                1.0 if v["kualitas_nilai"] >= 2 else 0.0,
            ),
            65,
        ),
        ("R-AKD-6", "IPS kurang", lambda m, v: M(m, "ips", "Kurang"), 95),
        (
            "R-AKD-7",
            "Ada nilai tidak lulus",
            lambda m, v: M(m, "kualitas_nilai", "Tidak Lulus"),
            95,
        ),
        (
            "R-AKD-8",
            "Mengulang mata kuliah",
            lambda m, v: M(m, "pengulangan", "Mengulang"),
            95,
        ),
    ],
    "kehadiran": [
        (
            "R-HDR-1",
            "Alfa, menit, dan jam semuanya sedikit",
            lambda m, v: AND(
                M(m, "alfa_mk", "Sedikit"),
                M(m, "akumulasi_menit", "Sedikit"),
                M(m, "jam_gabungan", "Sedikit"),
            ),
            10,
        ),
        (
            "R-HDR-2",
            "Alfa sedang, menit dan jam belum banyak",
            lambda m, v: AND(
                M(m, "alfa_mk", "Sedang"),
                NOT(
                    OR(
                        M(m, "akumulasi_menit", "Banyak (SP I)"),
                        M(m, "akumulasi_menit", "Kritis (SP II/III/DO)"),
                    )
                ),
                NOT(
                    OR(
                        M(m, "jam_gabungan", "Banyak"),
                        M(m, "jam_gabungan", "Melebihi Batas"),
                    )
                ),
            ),
            35,
        ),
        (
            "R-HDR-3",
            "Menit sedang, alfa dan jam belum banyak",
            lambda m, v: AND(
                M(m, "akumulasi_menit", "Sedang"),
                NOT(M(m, "alfa_mk", "Banyak")),
                NOT(
                    OR(
                        M(m, "jam_gabungan", "Banyak"),
                        M(m, "jam_gabungan", "Melebihi Batas"),
                    )
                ),
            ),
            35,
        ),
        (
            "R-HDR-4",
            "Jam sedang, alfa dan menit belum banyak",
            lambda m, v: AND(
                M(m, "jam_gabungan", "Sedang"),
                NOT(M(m, "alfa_mk", "Banyak")),
                NOT(
                    OR(
                        M(m, "akumulasi_menit", "Banyak (SP I)"),
                        M(m, "akumulasi_menit", "Kritis (SP II/III/DO)"),
                    )
                ),
            ),
            35,
        ),
        (
            "R-HDR-5",
            "Menit banyak, alfa belum banyak",
            lambda m, v: AND(
                M(m, "akumulasi_menit", "Banyak (SP I)"),
                NOT(M(m, "alfa_mk", "Banyak")),
            ),
            65,
        ),
        (
            "R-HDR-6",
            "Jam banyak, alfa belum banyak",
            lambda m, v: AND(
                M(m, "jam_gabungan", "Banyak"),
                NOT(M(m, "alfa_mk", "Banyak")),
            ),
            65,
        ),
        (
            "R-HDR-7",
            "Alfa banyak",
            lambda m, v: M(m, "alfa_mk", "Banyak"),
            95,
        ),
        (
            "R-HDR-8",
            "Menit kritis",
            lambda m, v: M(m, "akumulasi_menit", "Kritis (SP II/III/DO)"),
            95,
        ),
        (
            "R-HDR-9",
            "Jam melebihi batas",
            lambda m, v: M(m, "jam_gabungan", "Melebihi Batas"),
            95,
        ),
    ],
    "administrasi": [
        (
            "R-ADM-1",
            "Aktif normal dan masa studi longgar",
            lambda m, v: AND(
                M(m, "status", "Aktif Normal"),
                M(m, "sisa_masa_studi", "Longgar"),
            ),
            10,
        ),
        (
            "R-ADM-2",
            "Cuti non-akademik dan masa studi longgar",
            lambda m, v: AND(
                M(m, "status", "Cuti Non-Akademik"),
                M(m, "sisa_masa_studi", "Longgar"),
            ),
            35,
        ),
        (
            "R-ADM-3",
            "Cuti akademik",
            lambda m, v: M(m, "status", "Cuti Akademik"),
            65,
        ),
        (
            "R-ADM-4",
            "Sisa masa studi menipis",
            lambda m, v: M(m, "sisa_masa_studi", "Menipis"),
            65,
        ),
        (
            "R-ADM-5",
            "Mangkir satu semester",
            lambda m, v: M(m, "status", "Mangkir 1x"),
            95,
        ),
        (
            "R-ADM-6",
            "Mangkir beruntun",
            lambda m, v: M(m, "status", "Mangkir Beruntun"),
            95,
        ),
        (
            "R-ADM-7",
            "Sisa masa studi kritis",
            lambda m, v: M(m, "sisa_masa_studi", "Kritis"),
            95,
        ),
        (
            "R-ADM-8",
            "Sisa masa studi habis",
            lambda m, v: M(m, "sisa_masa_studi", "Habis"),
            95,
        ),
    ],
}


def kategori(skor):
    if skor is None:
        return None
    if skor <= 22.5:
        return AMAN
    if skor <= 50:
        return RENDAH
    if skor <= 80:
        return SEDANG
    return TINGGI


def worst(kategori_list):
    if not kategori_list:
        return TIDAK_ADA_DATA
    return max(kategori_list, key=lambda k: PROFIL.get(k, -1))


def evaluate(rules, values):
    mu = {k: fuzzify(k, v) for k, v in values.items()}
    aktif = []
    for rid, desc, fn, z in rules:
        alpha = max(0.0, min(1.0, fn(mu, values)))
        if alpha > 1e-9:
            aktif.append((rid, alpha, z, desc))
    total = sum(alpha for _, alpha, _, _ in aktif)
    skor = sum(alpha * z for _, alpha, z, _ in aktif) / total if total else None
    return {
        "nilai": values,
        "mu": mu,
        "aktif": aktif,
        "skor": skor,
        "kategori": kategori(skor),
    }


def infer(fitur):
    akademik = (
        evaluate(
            RULES["akademik"],
            {
                "ips": fitur["ips"],
                "tren_ips": fitur["delta_ips"],
                "kualitas_nilai": fitur["penalti"],
                "pengulangan": 1.0 if fitur["mengulang"] else 0.0,
            },
        )
        if fitur["ips"] is not None
        else None
    )
    kehadiran = (
        evaluate(
            RULES["kehadiran"],
            {
                "alfa_mk": fitur["max_alfa"],
                "akumulasi_menit": fitur["menit"],
                "jam_gabungan": fitur["jam"],
            },
        )
        if fitur["jumlah_mk"]
        else None
    )
    administrasi = (
        evaluate(
            RULES["administrasi"],
            {
                "status": fitur["status_kode"],
                "sisa_masa_studi": fitur["sisa"],
            },
        )
        if fitur["periode"]
        else None
    )

    subs = [
        x for x in (akademik, kehadiran, administrasi) if x and x["skor"] is not None
    ]
    skor = max((x["skor"] for x in subs), default=None)
    profil = worst([x["kategori"] for x in subs]) if subs else TIDAK_ADA_DATA
    return {
        "fitur": fitur,
        "akademik": akademik,
        "kehadiran": kehadiran,
        "administrasi": administrasi,
        "skor": skor,
        "profil": profil,
    }


def list_rules():
    labels = {
        "akademik": "Risiko Akademik",
        "kehadiran": "Risiko Kehadiran",
        "administrasi": "Risiko Administrasi",
    }
    lines = []
    for key in ("akademik", "kehadiran", "administrasi"):
        lines.append(labels[key])
        for rid, desc, _, z in RULES[key]:
            lines.append(f"  {rid}: {desc} -> {z:.0f}")
        lines.append("")
    lines.append("Agregasi: max(Z_akademik, Z_kehadiran, Z_administrasi)")
    return "\n".join(lines)
