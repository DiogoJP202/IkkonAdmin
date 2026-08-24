/* @ds-bundle: {"format":3,"namespace":"IKKONTaikoDesignSystem_825760","components":[{"name":"Divider","sourcePath":"components/brand/Divider.jsx"},{"name":"Logo","sourcePath":"components/brand/Logo.jsx"},{"name":"Seal","sourcePath":"components/brand/Seal.jsx"},{"name":"SectionTitle","sourcePath":"components/brand/SectionTitle.jsx"},{"name":"Badge","sourcePath":"components/core/Badge.jsx"},{"name":"Eyebrow","sourcePath":"components/core/Badge.jsx"},{"name":"Button","sourcePath":"components/core/Button.jsx"},{"name":"Card","sourcePath":"components/core/Card.jsx"},{"name":"Input","sourcePath":"components/core/Input.jsx"}],"sourceHashes":{"components/brand/Divider.jsx":"b559e2adfb62","components/brand/Logo.jsx":"c282b86a3aa5","components/brand/Seal.jsx":"a15b5896d583","components/brand/SectionTitle.jsx":"907de774ac09","components/core/Badge.jsx":"5b3d0923725b","components/core/Button.jsx":"8898988a1c38","components/core/Card.jsx":"945545e5fc7e","components/core/Input.jsx":"d99576eaf7ad","ui_kits/mobile_app/AgendaScreen.jsx":"a276d375e3b8","ui_kits/mobile_app/AppShell.jsx":"19b746df90c0","ui_kits/mobile_app/AulasScreen.jsx":"de2683de3843","ui_kits/mobile_app/HomeScreen.jsx":"f91a866a7d81","ui_kits/mobile_app/PerfilScreen.jsx":"410cd10182d9","ui_kits/website/SiteFooter.jsx":"4f7fbaca8be0","ui_kits/website/SiteHeader.jsx":"abc6f06152f1","ui_kits/website/SiteHero.jsx":"43b632ca435c","ui_kits/website/SiteSections.jsx":"4714490c616d"},"inlinedExternals":[],"unexposedExports":[]} */

(() => {

const __ds_ns = (window.IKKONTaikoDesignSystem_825760 = window.IKKONTaikoDesignSystem_825760 || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/brand/Divider.jsx
try { (() => {
function Divider({
  variant = 'line',
  onDark = false,
  height = 22,
  assetsBase,
  style
}) {
  const base = assetsBase || window.IKKON_ASSETS_BASE || 'assets';
  if (variant === 'brush') {
    return /*#__PURE__*/React.createElement("img", {
      src: `${base}/traco-pincel.png`,
      alt: "",
      style: {
        height,
        display: 'block',
        filter: onDark ? 'invert(1)' : 'none',
        ...style
      }
    });
  }
  if (variant === 'vertical') {
    return /*#__PURE__*/React.createElement("div", {
      style: {
        width: 1,
        alignSelf: 'stretch',
        background: onDark ? 'var(--border-on-dark)' : 'var(--border-on-light)',
        ...style
      }
    });
  }
  return /*#__PURE__*/React.createElement("hr", {
    style: {
      border: 'none',
      borderTop: `1px solid ${onDark ? 'var(--border-on-dark)' : 'var(--border-on-light)'}`,
      margin: 0,
      width: '100%',
      ...style
    }
  });
}
Object.assign(__ds_scope, { Divider });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/brand/Divider.jsx", error: String((e && e.message) || e) }); }

// components/brand/Logo.jsx
try { (() => {
function Logo({
  size = 'md',
  theme = 'dark',
  layout = 'horizontal',
  assetsBase,
  style
}) {
  const base = assetsBase || window.IKKON_ASSETS_BASE || 'assets';
  const heights = {
    sm: 44,
    md: 72,
    lg: 120
  };
  const h = typeof size === 'number' ? size : heights[size];
  const cream = 'var(--bege)',
    navy = 'var(--azul)';
  const ink = theme === 'dark' ? cream : navy;
  if (layout === 'emblem') {
    return /*#__PURE__*/React.createElement("img", {
      src: `${base}/pincelada-enso.png`,
      alt: "IKKON",
      style: {
        height: h,
        ...style
      }
    });
  }
  const stacked = layout === 'stacked';
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: stacked ? 'column' : 'row',
      alignItems: 'center',
      gap: h * 0.14,
      ...style
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: `${base}/pincelada-enso.png`,
    alt: "",
    style: {
      height: h * (stacked ? 0.62 : 0.96)
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      alignItems: stacked ? 'center' : 'flex-start',
      gap: h * 0.02
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-body)',
      fontWeight: 700,
      fontSize: Math.max(h * 0.115, 8),
      letterSpacing: '0.34em',
      color: 'var(--vermelho)',
      whiteSpace: 'nowrap',
      paddingLeft: '0.1em'
    }
  }, "S\xC3O PAULO TAIKO DOJO"), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-brand)',
      fontWeight: 700,
      fontSize: h * 0.56,
      lineHeight: 1,
      color: ink,
      letterSpacing: '0.02em'
    }
  }, "IKKON")));
}
Object.assign(__ds_scope, { Logo });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/brand/Logo.jsx", error: String((e && e.message) || e) }); }

// components/brand/Seal.jsx
try { (() => {
function Seal({
  variant = 'red',
  size = 64,
  assetsBase,
  style
}) {
  const base = assetsBase || window.IKKON_ASSETS_BASE || 'assets';
  const files = {
    red: 'selo-pequeno.png',
    'red-large': 'selo-ikkon.png',
    black: 'selo-kanji-preto.png',
    boxed: 'selo-caixa-vermelha.png'
  };
  return /*#__PURE__*/React.createElement("img", {
    src: `${base}/${files[variant]}`,
    alt: "Selo IKKON \u4E00\u9B42",
    style: {
      height: size,
      ...style
    }
  });
}
Object.assign(__ds_scope, { Seal });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/brand/Seal.jsx", error: String((e && e.message) || e) }); }

// components/brand/SectionTitle.jsx
try { (() => {
function SectionTitle({
  children,
  font = 'brand',
  color = 'var(--branco)',
  size = 30,
  assetsBase,
  style
}) {
  const base = assetsBase || window.IKKON_ASSETS_BASE || 'assets';
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      display: 'inline-grid',
      placeItems: 'center',
      padding: '0.45em 1.2em',
      ...style
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: `${base}/pincelada-titulo.png`,
    alt: "",
    style: {
      position: 'absolute',
      inset: 0,
      width: '100%',
      height: '100%'
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'relative',
      fontFamily: font === 'display' ? 'var(--font-display)' : 'var(--font-brand)',
      fontWeight: font === 'display' ? 400 : 700,
      fontSize: size,
      color,
      letterSpacing: '0.05em',
      whiteSpace: 'nowrap'
    }
  }, children));
}
Object.assign(__ds_scope, { SectionTitle });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/brand/SectionTitle.jsx", error: String((e && e.message) || e) }); }

// components/core/Badge.jsx
try { (() => {
function Badge({
  tone = 'red',
  caps = true,
  children,
  style
}) {
  const tones = {
    red: {
      background: 'var(--vermelho)',
      color: 'var(--bege)'
    },
    navy: {
      background: 'var(--azul)',
      color: 'var(--bege)'
    },
    outline: {
      background: 'transparent',
      color: 'var(--azul)',
      boxShadow: 'inset 0 0 0 1px var(--azul)'
    },
    'outline-dark': {
      background: 'transparent',
      color: 'var(--bege)',
      boxShadow: 'inset 0 0 0 1px var(--border-on-dark)'
    }
  };
  return /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-body)',
      fontWeight: 700,
      fontSize: 11,
      letterSpacing: caps ? '0.18em' : '0.04em',
      textTransform: caps ? 'uppercase' : 'none',
      padding: '5px 12px',
      display: 'inline-flex',
      alignItems: 'center',
      borderRadius: 0,
      ...tones[tone],
      ...style
    }
  }, children);
}
function Eyebrow({
  onDark = false,
  children,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontWeight: 700,
      fontSize: 12,
      letterSpacing: 'var(--tracking-caps)',
      textTransform: 'uppercase',
      color: onDark ? 'var(--vermelho)' : 'var(--vermelho)',
      ...style
    }
  }, children);
}
Object.assign(__ds_scope, { Badge, Eyebrow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Badge.jsx", error: String((e && e.message) || e) }); }

// components/core/Button.jsx
try { (() => {
function Button({
  variant = 'primary',
  size = 'md',
  arrow = false,
  disabled = false,
  children,
  onClick,
  style
}) {
  const [hover, setHover] = React.useState(false);
  const [press, setPress] = React.useState(false);
  const pad = size === 'sm' ? '8px 18px' : size === 'lg' ? '16px 34px' : '12px 26px';
  const font = size === 'sm' ? 13 : size === 'lg' ? 17 : 15;
  const base = {
    fontFamily: 'var(--font-body)',
    fontWeight: 700,
    fontSize: font,
    letterSpacing: '0.04em',
    padding: pad,
    cursor: disabled ? 'default' : 'pointer',
    display: 'inline-flex',
    alignItems: 'center',
    gap: 10,
    border: 'none',
    borderRadius: 'var(--radius-pill)',
    opacity: disabled ? 0.45 : 1,
    transition: 'background var(--duration-fast) var(--ease-brand), transform var(--duration-fast) var(--ease-brand)',
    transform: press && !disabled ? 'scale(0.98)' : 'none'
  };
  const variants = {
    primary: {
      background: hover && !disabled ? 'var(--accent-hover)' : 'var(--accent)',
      color: 'var(--text-on-accent)'
    },
    secondary: {
      background: hover && !disabled ? 'rgba(0,32,62,0.06)' : 'transparent',
      color: 'var(--azul)',
      boxShadow: 'inset 0 0 0 1.5px var(--azul)'
    },
    'secondary-dark': {
      background: hover && !disabled ? 'rgba(247,244,231,0.12)' : 'transparent',
      color: 'var(--bege)',
      boxShadow: 'inset 0 0 0 1.5px var(--bege)'
    },
    ghost: {
      background: 'transparent',
      color: 'var(--azul)',
      padding: '6px 2px',
      borderRadius: 0,
      textDecoration: hover && !disabled ? 'underline' : 'none'
    }
  };
  return /*#__PURE__*/React.createElement("button", {
    style: {
      ...base,
      ...variants[variant],
      ...style
    },
    disabled: disabled,
    onClick: onClick,
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => {
      setHover(false);
      setPress(false);
    },
    onMouseDown: () => setPress(true),
    onMouseUp: () => setPress(false)
  }, children, arrow && /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      fontFamily: 'var(--font-body)',
      fontWeight: 400,
      fontSize: font + 3,
      lineHeight: 1
    }
  }, "\u27F6"));
}
Object.assign(__ds_scope, { Button });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Button.jsx", error: String((e && e.message) || e) }); }

// components/core/Card.jsx
try { (() => {
function Card({
  surface = 'dark',
  framed = false,
  textured = false,
  padding = 24,
  assetsBase,
  children,
  style
}) {
  const base = assetsBase || window.IKKON_ASSETS_BASE || 'assets';
  const surfaces = {
    dark: {
      background: 'var(--azul)',
      color: 'var(--bege)'
    },
    darker: {
      background: 'var(--azul-escuro)',
      color: 'var(--bege)'
    },
    light: {
      background: 'var(--bege)',
      color: 'var(--azul)'
    },
    white: {
      background: 'var(--branco)',
      color: 'var(--azul)',
      boxShadow: 'var(--shadow-card)'
    },
    red: {
      background: 'var(--vermelho)',
      color: 'var(--bege)'
    }
  };
  const s = surfaces[surface];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      position: 'relative',
      overflow: 'hidden',
      borderRadius: 0,
      padding,
      border: framed ? '2px solid var(--vermelho)' : 'none',
      ...s,
      ...style
    }
  }, textured && (surface === 'dark' || surface === 'darker') && /*#__PURE__*/React.createElement("img", {
    src: `${base}/textura-pontos.png`,
    alt: "",
    style: {
      position: 'absolute',
      inset: 0,
      width: '100%',
      height: '100%',
      objectFit: 'cover',
      opacity: 0.5,
      pointerEvents: 'none'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative'
    }
  }, children));
}
Object.assign(__ds_scope, { Card });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Card.jsx", error: String((e && e.message) || e) }); }

// components/core/Input.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Input({
  label,
  type = 'text',
  placeholder,
  value,
  onChange,
  onDark = false,
  multiline = false,
  style
}) {
  const [focus, setFocus] = React.useState(false);
  const color = onDark ? 'var(--bege)' : 'var(--azul)';
  const border = focus ? '1.5px solid var(--vermelho)' : `1.5px solid ${onDark ? 'var(--border-on-dark)' : 'var(--border-on-light)'}`;
  const field = {
    fontFamily: 'var(--font-body)',
    fontSize: 15,
    color,
    background: onDark ? 'rgba(247,244,231,0.06)' : 'var(--branco)',
    border,
    borderRadius: 0,
    padding: '12px 14px',
    outline: 'none',
    width: '100%',
    boxSizing: 'border-box',
    transition: 'border-color var(--duration-fast) var(--ease-brand)',
    resize: 'vertical'
  };
  const shared = {
    placeholder,
    value,
    onChange,
    onFocus: () => setFocus(true),
    onBlur: () => setFocus(false),
    style: field
  };
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 6,
      fontFamily: 'var(--font-body)',
      ...style
    }
  }, label && /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 12,
      fontWeight: 700,
      letterSpacing: '0.14em',
      textTransform: 'uppercase',
      color,
      opacity: 0.75
    }
  }, label), multiline ? /*#__PURE__*/React.createElement("textarea", _extends({
    rows: 4
  }, shared)) : /*#__PURE__*/React.createElement("input", _extends({
    type: type
  }, shared)));
}
Object.assign(__ds_scope, { Input });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Input.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile_app/AgendaScreen.jsx
try { (() => {
function AgendaScreen() {
  const {
    Badge,
    Button
  } = window.IKKONTaikoDesignSystem_825760;
  const eventos = [{
    dia: '07',
    mes: 'DEZ',
    nome: 'Ensaio aberto — Ensemble',
    local: 'Unidade Patriarca',
    tag: 'Aberto'
  }, {
    dia: '13',
    mes: 'DEZ',
    nome: 'Festival do Japão — Apresentação',
    local: 'São Paulo Expo',
    tag: 'Show'
  }, {
    dia: '19',
    mes: 'DEZ',
    nome: 'Bounenkai — Festa de fim de ano',
    local: 'R. Trapiche, 182 · Patriarca',
    tag: 'Festa'
  }];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 16,
      padding: '10px 20px 24px'
    }
  }, /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-display)',
      fontSize: 28,
      fontWeight: 400,
      color: 'var(--bege)'
    }
  }, "AGENDA"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      fontWeight: 700,
      letterSpacing: 'var(--tracking-caps)',
      color: 'var(--vermelho)',
      textTransform: 'uppercase'
    }
  }, "Dezembro 2026"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column'
    }
  }, eventos.map((e, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: 'flex',
      gap: 16,
      padding: '16px 0',
      borderBottom: '1px solid var(--border-on-dark)',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      textAlign: 'center',
      minWidth: 52
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-brand)',
      fontWeight: 700,
      fontSize: 28,
      color: 'var(--vermelho)',
      lineHeight: 1
    }
  }, e.dia), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 10,
      fontWeight: 700,
      letterSpacing: '0.2em',
      color: 'var(--text-on-dark-soft)'
    }
  }, e.mes)), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontWeight: 700,
      fontSize: 14,
      color: 'var(--bege)',
      lineHeight: 1.35
    }
  }, e.nome), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      color: 'var(--text-on-dark-soft)',
      marginTop: 2
    }
  }, e.local)), /*#__PURE__*/React.createElement(Badge, {
    tone: "outline-dark"
  }, e.tag)))), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    arrow: true,
    style: {
      alignSelf: 'stretch',
      justifyContent: 'center'
    }
  }, "Quero assistir uma apresenta\xE7\xE3o"));
}
Object.assign(window, {
  AgendaScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile_app/AgendaScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile_app/AppShell.jsx
try { (() => {
function Icon({
  name,
  size = 22,
  color = 'currentColor',
  strokeWidth = 1.7,
  style
}) {
  const ref = React.useRef(null);
  React.useEffect(() => {
    if (ref.current && window.lucide) {
      ref.current.innerHTML = '';
      const el = document.createElement('i');
      el.setAttribute('data-lucide', name);
      ref.current.appendChild(el);
      window.lucide.createIcons({
        attrs: {
          width: size,
          height: size,
          'stroke-width': strokeWidth
        },
        root: ref.current
      });
    }
  }, [name, size, color]);
  return /*#__PURE__*/React.createElement("span", {
    ref: ref,
    style: {
      display: 'inline-flex',
      color,
      ...style
    }
  });
}
function StatusBar({
  onDark = true
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      padding: '14px 24px 6px',
      fontFamily: 'var(--font-body)',
      fontWeight: 700,
      fontSize: 14,
      color: onDark ? 'var(--bege)' : 'var(--azul)'
    }
  }, /*#__PURE__*/React.createElement("span", null, "9:41"), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'flex',
      gap: 6,
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: "signal",
    size: 15
  }), /*#__PURE__*/React.createElement(Icon, {
    name: "wifi",
    size: 15
  }), /*#__PURE__*/React.createElement(Icon, {
    name: "battery-full",
    size: 18
  })));
}
function TabBar({
  active,
  onSelect
}) {
  const tabs = [{
    id: 'home',
    label: 'Início',
    icon: 'home'
  }, {
    id: 'aulas',
    label: 'Aulas',
    icon: 'drum'
  }, {
    id: 'agenda',
    label: 'Agenda',
    icon: 'calendar'
  }, {
    id: 'perfil',
    label: 'Perfil',
    icon: 'user'
  }];
  return /*#__PURE__*/React.createElement("nav", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(4,1fr)',
      background: 'var(--azul-escuro)',
      borderTop: '1px solid var(--border-on-dark)',
      padding: '8px 0 20px'
    }
  }, tabs.map(t => /*#__PURE__*/React.createElement("button", {
    key: t.id,
    onClick: () => onSelect(t.id),
    style: {
      background: 'none',
      border: 'none',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 3,
      padding: '6px 0',
      minHeight: 44,
      color: active === t.id ? 'var(--vermelho)' : 'rgba(247,244,231,0.55)',
      fontFamily: 'var(--font-body)',
      fontSize: 10,
      fontWeight: 700,
      letterSpacing: '0.08em'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: t.icon,
    size: 22
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      textTransform: 'uppercase'
    }
  }, t.label))));
}
Object.assign(window, {
  Icon,
  StatusBar,
  TabBar
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile_app/AppShell.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile_app/AulasScreen.jsx
try { (() => {
function AulasScreen() {
  const {
    Badge
  } = window.IKKONTaikoDesignSystem_825760;
  const [unidade, setUnidade] = React.useState('Todas');
  const turmas = [{
    nome: 'Taiko Iniciante',
    nivel: 'Iniciante',
    dias: 'Seg · Qua · 19h00',
    unidade: 'Patriarca'
  }, {
    nome: 'Taiko Iniciante',
    nivel: 'Iniciante',
    dias: 'Ter · Qui · 19h00',
    unidade: 'Vila Mariana'
  }, {
    nome: 'Taiko Intermediário',
    nivel: 'Intermediário',
    dias: 'Ter · Qui · 20h30',
    unidade: 'Vila Mariana'
  }, {
    nome: 'Taiko Avançado',
    nivel: 'Avançado',
    dias: 'Sex · 20h00 · Sáb · 10h00',
    unidade: 'Patriarca'
  }, {
    nome: 'Preparatório Ensemble',
    nivel: 'Avançado',
    dias: 'Sáb · 14h00',
    unidade: 'Patriarca'
  }];
  const filtros = ['Todas', 'Patriarca', 'Vila Mariana'];
  const visiveis = turmas.filter(t => unidade === 'Todas' || t.unidade === unidade);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 16,
      padding: '10px 20px 24px'
    }
  }, /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-display)',
      fontSize: 28,
      fontWeight: 400,
      color: 'var(--bege)'
    }
  }, "AULAS"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 8
    }
  }, filtros.map(f => /*#__PURE__*/React.createElement("button", {
    key: f,
    onClick: () => setUnidade(f),
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      fontWeight: 700,
      letterSpacing: '0.06em',
      padding: '9px 16px',
      cursor: 'pointer',
      borderRadius: 'var(--radius-pill)',
      minHeight: 36,
      border: '1.5px solid ' + (unidade === f ? 'var(--vermelho)' : 'var(--border-on-dark)'),
      background: unidade === f ? 'var(--vermelho)' : 'transparent',
      color: unidade === f ? 'var(--bege)' : 'var(--text-on-dark-soft)',
      transition: 'all var(--duration-fast) var(--ease-brand)'
    }
  }, f))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 10
    }
  }, visiveis.map((t, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      background: 'var(--bege)',
      padding: '14px 16px',
      display: 'flex',
      flexDirection: 'column',
      gap: 6
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-body)',
      fontWeight: 900,
      fontSize: 15,
      color: 'var(--azul)'
    }
  }, t.nome), /*#__PURE__*/React.createElement(Badge, {
    tone: t.nivel === 'Iniciante' ? 'red' : t.nivel === 'Intermediário' ? 'navy' : 'outline'
  }, t.nivel)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      fontFamily: 'var(--font-body)',
      fontSize: 12.5,
      color: 'var(--text-body-soft)'
    }
  }, /*#__PURE__*/React.createElement("span", null, t.dias), /*#__PURE__*/React.createElement("span", {
    style: {
      fontWeight: 700
    }
  }, t.unidade))))), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      color: 'var(--text-on-dark-soft)',
      lineHeight: 1.5
    }
  }, "Aulas de segunda a s\xE1bado nas unidades ", /*#__PURE__*/React.createElement("b", {
    style: {
      color: 'var(--bege)'
    }
  }, "Patriarca"), " e ", /*#__PURE__*/React.createElement("b", {
    style: {
      color: 'var(--bege)'
    }
  }, "Vila Mariana"), "."));
}
Object.assign(window, {
  AulasScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile_app/AulasScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile_app/HomeScreen.jsx
try { (() => {
function HomeScreen({
  onGo
}) {
  const {
    Badge,
    Button,
    Seal
  } = window.IKKONTaikoDesignSystem_825760;
  const aulas = [{
    hora: '19h00',
    nome: 'Taiko Iniciante',
    unidade: 'Vila Mariana',
    vagas: true
  }, {
    hora: '20h30',
    nome: 'Taiko Intermediário',
    unidade: 'Vila Mariana',
    vagas: false
  }];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 20,
      padding: '10px 20px 24px'
    }
  }, /*#__PURE__*/React.createElement("header", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      fontWeight: 700,
      letterSpacing: 'var(--tracking-caps)',
      color: 'var(--vermelho)',
      textTransform: 'uppercase'
    }
  }, "S\xE3o Paulo Taiko Dojo"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 26,
      color: 'var(--bege)',
      marginTop: 4
    }
  }, "OL\xC1, MARINA")), /*#__PURE__*/React.createElement(Seal, {
    size: 44
  })), /*#__PURE__*/React.createElement("section", {
    style: {
      background: 'var(--vermelho)',
      padding: 18,
      display: 'flex',
      flexDirection: 'column',
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 11,
      fontWeight: 700,
      letterSpacing: '0.18em',
      color: 'var(--bege)',
      textTransform: 'uppercase'
    }
  }, "Pr\xF3ximo evento"), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-brand)',
      fontWeight: 700,
      fontSize: 20,
      color: 'var(--bege)'
    }
  }, "19/12")), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 30,
      color: 'var(--bege)'
    }
  }, "BOUNENKAI"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 13,
      color: 'rgba(247,244,231,0.85)',
      lineHeight: 1.5
    }
  }, "Festa japonesa de fim de ano. R. Trapiche, 182 \xB7 Patriarca."), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary-dark",
    size: "sm",
    arrow: true,
    onClick: () => onGo('agenda'),
    style: {
      alignSelf: 'flex-start',
      marginTop: 4
    }
  }, "Confirmar presen\xE7a")), /*#__PURE__*/React.createElement("section", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'baseline'
    }
  }, /*#__PURE__*/React.createElement("h2", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-display)',
      fontSize: 18,
      fontWeight: 400,
      color: 'var(--bege)',
      letterSpacing: '0.04em'
    }
  }, "AULAS DE HOJE"), /*#__PURE__*/React.createElement("button", {
    onClick: () => onGo('aulas'),
    style: {
      background: 'none',
      border: 'none',
      cursor: 'pointer',
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      fontWeight: 700,
      color: 'var(--vermelho)',
      letterSpacing: '0.08em'
    }
  }, "VER TODAS \u27F6")), aulas.map(a => /*#__PURE__*/React.createElement("div", {
    key: a.hora,
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 14,
      background: 'rgba(247,244,231,0.05)',
      border: '1px solid var(--border-on-dark)',
      padding: '12px 14px'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-brand)',
      fontWeight: 700,
      fontSize: 17,
      color: 'var(--vermelho)',
      minWidth: 54
    }
  }, a.hora), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontWeight: 700,
      fontSize: 14,
      color: 'var(--bege)'
    }
  }, a.nome), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      color: 'var(--text-on-dark-soft)'
    }
  }, a.unidade)), /*#__PURE__*/React.createElement(Badge, {
    tone: a.vagas ? 'red' : 'outline-dark'
  }, a.vagas ? 'Vagas' : 'Cheia')))), /*#__PURE__*/React.createElement("section", {
    style: {
      position: 'relative',
      overflow: 'hidden',
      background: 'var(--azul-escuro)',
      padding: 18
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/pincelada-enso-navy.png",
    alt: "",
    style: {
      position: 'absolute',
      right: -70,
      top: -60,
      width: 220,
      opacity: 0.5,
      filter: 'brightness(3)'
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      display: 'flex',
      flexDirection: 'column',
      gap: 6
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 11,
      fontWeight: 700,
      letterSpacing: '0.18em',
      color: 'var(--vermelho)',
      textTransform: 'uppercase'
    }
  }, "Conhe\xE7a o taiko"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-display)',
      fontSize: 20,
      color: 'var(--bege)'
    }
  }, "AULA EXPERIMENTAL GRATUITA"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 13,
      color: 'var(--text-on-dark-soft)',
      lineHeight: 1.5
    }
  }, "Convide um amigo para sentir a for\xE7a dos tambores japoneses."))));
}
Object.assign(window, {
  HomeScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile_app/HomeScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/mobile_app/PerfilScreen.jsx
try { (() => {
function PerfilScreen() {
  const {
    Badge,
    Divider,
    Seal
  } = window.IKKONTaikoDesignSystem_825760;
  const linhas = [{
    icon: 'calendar-check',
    label: 'Minhas presenças',
    extra: '32 este ano'
  }, {
    icon: 'credit-card',
    label: 'Mensalidade',
    extra: 'Em dia'
  }, {
    icon: 'bell',
    label: 'Notificações',
    extra: ''
  }, {
    icon: 'circle-help',
    label: 'Fale com o dojo',
    extra: ''
  }];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 18,
      padding: '10px 20px 24px'
    }
  }, /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-display)',
      fontSize: 28,
      fontWeight: 400,
      color: 'var(--bege)'
    }
  }, "PERFIL"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 16,
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: 72,
      height: 72,
      borderRadius: '50%',
      background: 'var(--bege)',
      display: 'grid',
      placeItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-brand)',
      fontWeight: 700,
      fontSize: 26,
      color: 'var(--azul)'
    }
  }, "M")), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontWeight: 900,
      fontSize: 18,
      color: 'var(--bege)'
    }
  }, "Marina Sato"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12.5,
      color: 'var(--text-on-dark-soft)'
    }
  }, "Turma Intermedi\xE1rio \xB7 Vila Mariana"), /*#__PURE__*/React.createElement(Badge, {
    tone: "red",
    style: {
      marginTop: 6
    }
  }, "Aluna desde 2023")), /*#__PURE__*/React.createElement(Seal, {
    size: 40
  })), /*#__PURE__*/React.createElement(Divider, {
    onDark: true
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column'
    }
  }, linhas.map((l, i) => /*#__PURE__*/React.createElement("button", {
    key: i,
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 14,
      padding: '15px 2px',
      minHeight: 48,
      background: 'none',
      border: 'none',
      borderBottom: '1px solid var(--border-on-dark)',
      cursor: 'pointer',
      textAlign: 'left'
    }
  }, /*#__PURE__*/React.createElement(Icon, {
    name: l.icon,
    size: 20,
    color: "var(--vermelho)"
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1,
      fontFamily: 'var(--font-body)',
      fontWeight: 700,
      fontSize: 14,
      color: 'var(--bege)'
    }
  }, l.label), l.extra && /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      color: 'var(--text-on-dark-soft)'
    }
  }, l.extra), /*#__PURE__*/React.createElement(Icon, {
    name: "chevron-right",
    size: 18,
    color: "rgba(247,244,231,0.4)"
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 11,
      letterSpacing: '0.2em',
      color: 'rgba(247,244,231,0.4)',
      textAlign: 'center',
      textTransform: 'uppercase'
    }
  }, "Ikkon S\xE3o Paulo Taiko Dojo \xB7 desde 2015"));
}
Object.assign(window, {
  PerfilScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/mobile_app/PerfilScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/website/SiteFooter.jsx
try { (() => {
function SiteUnidades() {
  const {
    Eyebrow
  } = window.IKKONTaikoDesignSystem_825760;
  const unidades = [{
    nome: 'Patriarca',
    end: 'R. Trapiche, 182 — Cidade Patriarca, São Paulo · SP'
  }, {
    nome: 'Vila Mariana',
    end: 'Vila Mariana, São Paulo · SP'
  }];
  return /*#__PURE__*/React.createElement("section", {
    style: {
      background: 'var(--bege)',
      padding: '96px 64px',
      display: 'flex',
      flexDirection: 'column',
      gap: 36
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 12
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Onde estamos"), /*#__PURE__*/React.createElement("h2", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-brand)',
      fontWeight: 700,
      fontSize: 40,
      color: 'var(--azul)'
    }
  }, "Duas unidades em S\xE3o Paulo")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 20
    }
  }, unidades.map(u => /*#__PURE__*/React.createElement("div", {
    key: u.nome,
    style: {
      background: 'var(--branco)',
      border: '2px solid var(--vermelho)',
      padding: 28,
      display: 'flex',
      gap: 20,
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/selo-pequeno.png",
    alt: "",
    style: {
      height: 54
    }
  }), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-brand)',
      fontWeight: 700,
      fontSize: 22,
      color: 'var(--azul)'
    }
  }, u.nome), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 14.5,
      color: 'var(--text-body-soft)',
      marginTop: 4
    }
  }, u.end))))));
}
function SiteContato() {
  const {
    Button,
    Input
  } = window.IKKONTaikoDesignSystem_825760;
  return /*#__PURE__*/React.createElement("section", {
    style: {
      background: 'var(--vermelho)',
      padding: '80px 64px',
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 64,
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 14
    }
  }, /*#__PURE__*/React.createElement("h2", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-display)',
      fontWeight: 400,
      fontSize: 42,
      color: 'var(--bege)',
      lineHeight: 1.12
    }
  }, "SINTA A FOR\xC7A DO TAIKO"), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-body)',
      fontSize: 16,
      lineHeight: 1.6,
      color: 'rgba(247,244,231,0.9)',
      maxWidth: 440
    }
  }, "Agende uma aula experimental e conhe\xE7a o dojo mais perto de voc\xEA.")), /*#__PURE__*/React.createElement("form", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 14,
      background: 'var(--bege)',
      padding: 32
    },
    onSubmit: e => e.preventDefault()
  }, /*#__PURE__*/React.createElement(Input, {
    label: "Nome",
    placeholder: "Seu nome completo"
  }), /*#__PURE__*/React.createElement(Input, {
    label: "WhatsApp",
    placeholder: "(11) 90000-0000"
  }), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    arrow: true,
    style: {
      justifyContent: 'center'
    }
  }, "Agendar aula experimental")));
}
function SiteFooter() {
  const {
    Logo,
    Divider
  } = window.IKKONTaikoDesignSystem_825760;
  return /*#__PURE__*/React.createElement("footer", {
    style: {
      background: 'var(--azul-escuro)',
      padding: '64px 64px 40px',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: 28
    }
  }, /*#__PURE__*/React.createElement(Logo, {
    theme: "dark",
    layout: "stacked",
    size: 120
  }), /*#__PURE__*/React.createElement("nav", {
    style: {
      display: 'flex',
      gap: 28
    }
  }, ['A Escola', 'Aulas', 'Ensemble', 'Agenda', 'Contato'].map(l => /*#__PURE__*/React.createElement("a", {
    key: l,
    href: "#",
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      fontWeight: 700,
      letterSpacing: '0.14em',
      textTransform: 'uppercase',
      color: 'var(--text-on-dark-soft)',
      textDecoration: 'none'
    }
  }, l))), /*#__PURE__*/React.createElement(Divider, {
    onDark: true,
    style: {
      width: 320
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12,
      color: 'rgba(247,244,231,0.5)',
      letterSpacing: '0.06em',
      textAlign: 'center'
    }
  }, "IKKON S\xE3o Paulo Taiko Dojo \xB7 Patriarca e Vila Mariana \xB7 S\xE3o Paulo, Brasil"));
}
Object.assign(window, {
  SiteUnidades,
  SiteContato,
  SiteFooter
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/website/SiteFooter.jsx", error: String((e && e.message) || e) }); }

// ui_kits/website/SiteHeader.jsx
try { (() => {
function SiteHeader({
  onNav
}) {
  const {
    Logo,
    Button
  } = window.IKKONTaikoDesignSystem_825760;
  const links = ['A Escola', 'Aulas', 'Ensemble', 'Agenda', 'Contato'];
  const [hover, setHover] = React.useState(null);
  return /*#__PURE__*/React.createElement("header", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '18px 64px',
      background: 'var(--azul-escuro)',
      position: 'relative',
      zIndex: 2
    }
  }, /*#__PURE__*/React.createElement(Logo, {
    theme: "dark",
    size: 46
  }), /*#__PURE__*/React.createElement("nav", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 32
    }
  }, links.map(l => /*#__PURE__*/React.createElement("a", {
    key: l,
    href: "#",
    onClick: e => {
      e.preventDefault();
      onNav && onNav(l);
    },
    onMouseEnter: () => setHover(l),
    onMouseLeave: () => setHover(null),
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 13,
      fontWeight: 700,
      letterSpacing: '0.12em',
      textTransform: 'uppercase',
      color: hover === l ? 'var(--vermelho)' : 'var(--bege)',
      textDecoration: 'none',
      transition: 'color var(--duration-fast) var(--ease-brand)'
    }
  }, l)), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "sm"
  }, "Aula experimental")));
}
Object.assign(window, {
  SiteHeader
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/website/SiteHeader.jsx", error: String((e && e.message) || e) }); }

// ui_kits/website/SiteHero.jsx
try { (() => {
function SiteHero() {
  const {
    Button,
    Eyebrow
  } = window.IKKONTaikoDesignSystem_825760;
  return /*#__PURE__*/React.createElement("section", {
    style: {
      position: 'relative',
      background: 'var(--azul)',
      overflow: 'hidden',
      padding: '96px 64px 110px'
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/textura-pontos.png",
    alt: "",
    style: {
      position: 'absolute',
      inset: 0,
      width: '100%',
      height: '100%',
      objectFit: 'cover',
      opacity: 0.5
    }
  }), /*#__PURE__*/React.createElement("img", {
    src: "../../assets/pincelada-enso.png",
    alt: "",
    style: {
      position: 'absolute',
      right: -60,
      top: '50%',
      transform: 'translateY(-50%)',
      height: 560,
      opacity: 0.9
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      maxWidth: 720,
      display: 'flex',
      flexDirection: 'column',
      gap: 24
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "S\xE3o Paulo Taiko Dojo \xB7 desde 2015"), /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-display)',
      fontWeight: 400,
      fontSize: 64,
      lineHeight: 1.08,
      color: 'var(--bege)',
      letterSpacing: '0.02em',
      textWrap: 'balance'
    }
  }, "A ARTE DOS TAMBORES JAPONESES"), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-body)',
      fontSize: 19,
      lineHeight: 1.6,
      color: 'var(--text-on-dark-soft)',
      maxWidth: 540
    }
  }, "Escola dedicada ao ensino e \xE0 pesquisa do taiko, unindo as bases tradicionais \xE0 teoria musical moderna. Mais de 120 alunos, aulas de segunda a s\xE1bado."), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 16,
      marginTop: 8
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "lg",
    arrow: true
  }, "Agende uma aula experimental"), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary-dark",
    size: "lg"
  }, "Conhe\xE7a o Ensemble"))));
}
function SiteStats() {
  const stats = [{
    n: '2015',
    l: 'Fundação'
  }, {
    n: '120+',
    l: 'Alunos matriculados'
  }, {
    n: '2',
    l: 'Unidades em SP'
  }, {
    n: 'SEG–SÁB',
    l: 'Aulas toda semana'
  }];
  return /*#__PURE__*/React.createElement("section", {
    style: {
      background: 'var(--vermelho)',
      display: 'grid',
      gridTemplateColumns: 'repeat(4,1fr)',
      padding: '36px 64px'
    }
  }, stats.map((s, i) => /*#__PURE__*/React.createElement("div", {
    key: s.l,
    style: {
      textAlign: 'center',
      borderLeft: i ? '1px solid rgba(247,244,231,0.3)' : 'none',
      padding: '4px 12px'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-brand)',
      fontWeight: 700,
      fontSize: 34,
      color: 'var(--bege)'
    }
  }, s.n), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 12.5,
      fontWeight: 700,
      letterSpacing: '0.14em',
      textTransform: 'uppercase',
      color: 'rgba(247,244,231,0.85)',
      marginTop: 4
    }
  }, s.l))));
}
Object.assign(window, {
  SiteHero,
  SiteStats
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/website/SiteHero.jsx", error: String((e && e.message) || e) }); }

// ui_kits/website/SiteSections.jsx
try { (() => {
function FotoPlaceholder({
  height = 380,
  label = 'FOTO DO DOJO'
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      height,
      display: 'grid',
      placeItems: 'center',
      overflow: 'hidden'
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/sol-pincelada.png",
    alt: "",
    style: {
      position: 'absolute',
      height: height * 0.94
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      position: 'relative',
      fontFamily: 'var(--font-body)',
      fontSize: 11,
      fontWeight: 700,
      letterSpacing: '0.22em',
      color: 'var(--azul)',
      background: 'var(--bege)',
      padding: '6px 12px',
      border: '1px dashed var(--azul)'
    }
  }, label, " \xB7 SUBSTITUIR"));
}
function SiteSobre() {
  const {
    Eyebrow
  } = window.IKKONTaikoDesignSystem_825760;
  return /*#__PURE__*/React.createElement("section", {
    style: {
      background: 'var(--bege)',
      padding: '96px 64px',
      display: 'grid',
      gridTemplateColumns: '1.1fr 1fr',
      gap: 64,
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 20
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "A Escola"), /*#__PURE__*/React.createElement("h2", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-brand)',
      fontWeight: 700,
      fontSize: 40,
      color: 'var(--azul)',
      lineHeight: 1.15
    }
  }, "Uma d\xE9cada de ", /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--vermelho)'
    }
  }, "taiko"), " em S\xE3o Paulo"), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-body)',
      fontSize: 16.5,
      lineHeight: 1.65,
      color: 'var(--text-body-soft)'
    }
  }, "Fundado em 2015, o ", /*#__PURE__*/React.createElement("b", {
    style: {
      fontFamily: 'var(--font-brand)',
      color: 'var(--azul)'
    }
  }, "IKKON"), " S\xE3o Paulo Taiko Dojo consolidou uma metodologia pr\xF3pria que une o ensino das bases tradicionais do taiko \xE0 teoria musical moderna."), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-body)',
      fontSize: 16.5,
      lineHeight: 1.65,
      color: 'var(--text-body-soft)'
    }
  }, "S\xE3o mais de 120 alunos matriculados, com aulas de segunda a s\xE1bado nas unidades ", /*#__PURE__*/React.createElement("b", null, "Patriarca"), " e ", /*#__PURE__*/React.createElement("b", null, "Vila Mariana"), ".")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gap: 16
    }
  }, /*#__PURE__*/React.createElement(FotoPlaceholder, null), /*#__PURE__*/React.createElement("img", {
    src: "../../assets/ilustracao-torii-fuji.png",
    alt: "",
    style: {
      height: 110,
      justifySelf: 'end'
    }
  })));
}
function SiteAulas() {
  const {
    Badge,
    Button
  } = window.IKKONTaikoDesignSystem_825760;
  const turmas = [{
    nivel: 'Iniciante',
    desc: 'Primeiros golpes, postura e kata. Nenhuma experiência musical necessária.',
    dias: 'Seg · Qua · Ter · Qui'
  }, {
    nivel: 'Intermediário',
    desc: 'Repertório tradicional, leitura rítmica e dinâmica de grupo.',
    dias: 'Ter · Qui'
  }, {
    nivel: 'Avançado',
    desc: 'Peças contemporâneas, composição e preparação para o Ensemble.',
    dias: 'Sex · Sáb'
  }];
  return /*#__PURE__*/React.createElement("section", {
    style: {
      background: 'var(--azul)',
      position: 'relative',
      overflow: 'hidden',
      padding: '96px 64px'
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/textura-pontos.png",
    alt: "",
    style: {
      position: 'absolute',
      inset: 0,
      width: '100%',
      height: '100%',
      objectFit: 'cover',
      opacity: 0.4
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'relative',
      display: 'flex',
      flexDirection: 'column',
      gap: 40
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'flex-end'
    }
  }, /*#__PURE__*/React.createElement("h2", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-display)',
      fontWeight: 400,
      fontSize: 44,
      color: 'var(--bege)'
    }
  }, "AULAS"), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 13,
      letterSpacing: '0.18em',
      textTransform: 'uppercase',
      color: 'var(--text-on-dark-soft)'
    }
  }, "Patriarca \xB7 Vila Mariana")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(3,1fr)',
      gap: 20
    }
  }, turmas.map(t => /*#__PURE__*/React.createElement("div", {
    key: t.nivel,
    style: {
      background: 'var(--bege)',
      padding: 28,
      display: 'flex',
      flexDirection: 'column',
      gap: 14
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: "red",
    style: {
      alignSelf: 'flex-start'
    }
  }, t.nivel), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-body)',
      fontSize: 15,
      lineHeight: 1.6,
      color: 'var(--text-body-soft)',
      flex: 1
    }
  }, t.desc), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--font-body)',
      fontSize: 13,
      fontWeight: 700,
      color: 'var(--azul)',
      letterSpacing: '0.06em'
    }
  }, t.dias)))), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    arrow: true,
    style: {
      alignSelf: 'flex-start'
    }
  }, "Quero come\xE7ar no taiko")));
}
function SiteEnsemble() {
  const {
    Eyebrow,
    Button
  } = window.IKKONTaikoDesignSystem_825760;
  return /*#__PURE__*/React.createElement("section", {
    style: {
      background: 'var(--azul-escuro)',
      padding: '96px 64px',
      display: 'grid',
      gridTemplateColumns: '1fr 1.1fr',
      gap: 64,
      alignItems: 'center'
    }
  }, /*#__PURE__*/React.createElement(FotoPlaceholder, {
    height: 420,
    label: "FOTO DO ENSEMBLE"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 20
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, null, "Ikkon Taiko Ensemble"), /*#__PURE__*/React.createElement("h2", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-display)',
      fontWeight: 400,
      fontSize: 44,
      color: 'var(--bege)',
      lineHeight: 1.1
    }
  }, "ENTRE OS PRINCIPAIS GRUPOS DE TAIKO DO BRASIL"), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      fontFamily: 'var(--font-body)',
      fontSize: 16.5,
      lineHeight: 1.65,
      color: 'var(--text-on-dark-soft)'
    }
  }, "Grupo art\xEDstico formado por m\xFAsicos experientes, com repert\xF3rio de pe\xE7as tradicionais, composi\xE7\xF5es contempor\xE2neas e obras autorais. Presente nos maiores festivais de cultura japonesa de S\xE3o Paulo e em eventos corporativos."), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary-dark",
    arrow: true,
    style: {
      alignSelf: 'flex-start'
    }
  }, "Contrate para seu evento")));
}
Object.assign(window, {
  FotoPlaceholder,
  SiteSobre,
  SiteAulas,
  SiteEnsemble
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/website/SiteSections.jsx", error: String((e && e.message) || e) }); }

__ds_ns.Divider = __ds_scope.Divider;

__ds_ns.Logo = __ds_scope.Logo;

__ds_ns.Seal = __ds_scope.Seal;

__ds_ns.SectionTitle = __ds_scope.SectionTitle;

__ds_ns.Badge = __ds_scope.Badge;

__ds_ns.Eyebrow = __ds_scope.Eyebrow;

__ds_ns.Button = __ds_scope.Button;

__ds_ns.Card = __ds_scope.Card;

__ds_ns.Input = __ds_scope.Input;

})();
