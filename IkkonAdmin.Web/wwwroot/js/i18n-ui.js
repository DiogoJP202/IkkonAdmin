(() => {
  const htmlLang = document.documentElement.lang || "";
  if (!htmlLang.toLowerCase().startsWith("en")) {
    return;
  }

  const dictionary = new Map([
    ["Abrir", "Open"],
    ["Abrir configurações", "Open settings"],
    ["Abrir financeiro", "Open finance"],
    ["Acesso", "Access"],
    ["Acesso administrativo para funcionários e administradores.", "Administrative access for staff and administrators."],
    ["Acesso negado", "Access denied"],
    ["Acessos", "Access"],
    ["Acessos recentes", "Recent access"],
    ["Ações", "Actions"],
    ["Adicionar", "Add"],
    ["Admissão", "Admission"],
    ["Admissões", "Admissions"],
    ["Agenda", "Calendar"],
    ["Agenda de cobrança", "Billing agenda"],
    ["Agendar aula experimental", "Schedule a trial class"],
    ["Aluno", "Student"],
    ["Alunos", "Students"],
    ["Alunos ativos", "Active students"],
    ["Antes de salvar", "Before saving"],
    ["Aplicar filtros", "Apply filters"],
    ["Apresentações", "Performances"],
    ["Área do aluno", "Student area"],
    ["Área do Aluno", "Student Area"],
    ["Área interna", "Staff area"],
    ["Atalhos", "Shortcuts"],
    ["Ativa", "Active"],
    ["Ativo", "Active"],
    ["Atividades recentes", "Recent activity"],
    ["Atrasadas", "Overdue"],
    ["Atrasados", "Overdue"],
    ["Atualizando...", "Updating..."],
    ["Baixar", "Download"],
    ["Blog", "Blog"],
    ["Boas práticas", "Best practices"],
    ["Buscar", "Search"],
    ["Buscar aluno", "Search student"],
    ["Cadastrar", "Register"],
    ["Cadastrar aluno", "Register student"],
    ["Cadastrar aula", "Register class"],
    ["Categoria", "Category"],
    ["Central operacional", "Operations center"],
    ["Claro", "Light"],
    ["Cobranças", "Charges"],
    ["Cobrar", "Charge"],
    ["Comunicados", "Announcements"],
    ["Comunicados ativos", "Active announcements"],
    ["Competência", "Period"],
    ["Configurações", "Settings"],
    ["Confirmar", "Confirm"],
    ["Confirmar nova senha", "Confirm new password"],
    ["Conquistas", "Achievements"],
    ["Conquistas no mês", "Achievements this month"],
    ["Conta", "Account"],
    ["Conta e preferências", "Account and preferences"],
    ["Conta e senha", "Account and password"],
    ["Contato", "Contact"],
    ["Contato de emergência", "Emergency contact"],
    ["Criar", "Create"],
    ["Dashboard administrativo", "Administrative dashboard"],
    ["Data", "Date"],
    ["Data de nascimento", "Birth date"],
    ["Data e hora", "Date and time"],
    ["Dados cadastrais", "Registration data"],
    ["Dados da conta", "Account data"],
    ["Dados principais", "Main data"],
    ["Desligado", "Inactive"],
    ["Desligamentos", "Deactivations"],
    ["Detalhes", "Details"],
    ["Detalhes do Aluno", "Student Details"],
    ["Dia", "Day"],
    ["Documentos", "Documents"],
    ["Documentos pendentes", "Pending documents"],
    ["Documentos recentes", "Recent documents"],
    ["E-mail", "Email"],
    ["Editar", "Edit"],
    ["Editar cadastro", "Edit registration"],
    ["Editar Aluno", "Edit Student"],
    ["Em admissão", "In admission"],
    ["Endereço", "Address"],
    ["Entrar", "Sign in"],
    ["Entrar na Área do Aluno", "Sign in to Student Area"],
    ["Entrar no IkkonAdmin", "Sign in to IkkonAdmin"],
    ["Entrar no portal", "Sign in to the portal"],
    ["Enviar", "Send"],
    ["Enviar documentos", "Send documents"],
    ["Escola", "School"],
    ["Escuro", "Dark"],
    ["Evento", "Event"],
    ["Eventos", "Events"],
    ["Eventos próximos", "Upcoming events"],
    ["Excluir", "Delete"],
    ["Fechar", "Close"],
    ["Filtrar", "Filter"],
    ["Financeiro", "Finance"],
    ["Forma", "Method"],
    ["Frequência", "Attendance"],
    ["Frequências no mês", "Attendance records this month"],
    ["Gerar mensalidades", "Generate monthly fees"],
    ["Graduações", "Graduations"],
    ["Histórico", "History"],
    ["Histórico financeiro", "Financial history"],
    ["Horário", "Schedule"],
    ["Horários cadastrados", "Registered schedules"],
    ["Idioma", "Language"],
    ["Inadimplência", "Overdue payments"],
    ["Inadimplentes", "Overdue students"],
    ["Inativa", "Inactive"],
    ["Inativo", "Inactive"],
    ["Início", "Home"],
    ["Instrutor", "Instructor"],
    ["Instrutor principal", "Main instructor"],
    ["Inventário", "Inventory"],
    ["Limpar", "Clear"],
    ["Limpar filtros", "Clear filters"],
    ["Local", "Location"],
    ["Marcar como lido", "Mark as read"],
    ["Mensalidade", "Monthly fee"],
    ["Mensalidades recentes", "Recent monthly fees"],
    ["Mês", "Month"],
    ["Meu perfil", "My profile"],
    ["Minhas Mensalidades", "My Monthly Fees"],
    ["Minhas Turmas", "My Groups"],
    ["Nome completo", "Full name"],
    ["Nova aula", "New class"],
    ["Nova senha", "New password"],
    ["Novo", "New"],
    ["Novo aluno", "New student"],
    ["Novo horário", "New schedule"],
    ["Observação", "Note"],
    ["Observações", "Notes"],
    ["Pagamento", "Payment"],
    ["Painel administrativo interno", "Internal admin panel"],
    ["Pendentes", "Pending"],
    ["Perfil", "Profile"],
    ["Permissões", "Permissions"],
    ["Permissões básicas", "Basic permissions"],
    ["Pesquisar", "Search"],
    ["Preferências", "Preferences"],
    ["Presença", "Attendance"],
    ["Próximas aulas", "Upcoming classes"],
    ["Próximos eventos", "Upcoming events"],
    ["Publicado em", "Published on"],
    ["Receita recebida", "Revenue received"],
    ["Registrar", "Register"],
    ["Registrar pagamento", "Register payment"],
    ["Resumo da conta", "Account summary"],
    ["Sair", "Sign out"],
    ["Salvar", "Save"],
    ["Salvar alterações", "Save changes"],
    ["Salvar exame", "Save exam"],
    ["Salvar horário", "Save schedule"],
    ["Salvar preferências", "Save preferences"],
    ["Salvando...", "Saving..."],
    ["Segurança", "Security"],
    ["Segurança diária", "Daily security"],
    ["Selecionar", "Select"],
    ["Selecione", "Select"],
    ["Sem instrutor", "No instructor"],
    ["Sem registro", "No record"],
    ["Sem turma", "No group"],
    ["Senha atual", "Current password"],
    ["Status da conta", "Account status"],
    ["Telefone", "Phone"],
    ["Tema", "Theme"],
    ["Tipo", "Type"],
    ["Tipo de acesso", "Access type"],
    ["Tipo de conta", "Account type"],
    ["Todas", "All"],
    ["Todas as categorias", "All categories"],
    ["Todas as turmas", "All groups"],
    ["Todos", "All"],
    ["Total em aberto", "Open balance"],
    ["Total pago", "Total paid"],
    ["Turma", "Group"],
    ["Turmas", "Groups"],
    ["Último login", "Last login"],
    ["Valor", "Amount"],
    ["Vencimento", "Due date"],
    ["Ver aulas", "View classes"],
    ["Ver atrasados", "View overdue"],
    ["Ver detalhes", "View details"],
    ["Ver eventos", "View events"],
    ["Ver financeiro", "View finance"],
    ["Ver todos", "View all"],
    ["Vincular", "Link"],
    ["Vincular instrutor", "Link instructor"],
    ["Visão geral", "Overview"],
    ["Voltar", "Back"],
    ["Voltar para lista", "Back to list"],
    ["Voltar para o painel", "Back to dashboard"],
    ["Voltar para o site", "Back to site"]
  ]);

  const normalize = (value) => value.replace(/\s+/g, " ").trim();
  const translate = (value) => dictionary.get(normalize(value));
  const skippedParents = new Set(["SCRIPT", "STYLE", "TEXTAREA", "CODE", "PRE"]);

  document.querySelectorAll("input, textarea, select, button, a, img, [title], [aria-label], [data-loading-text]").forEach((element) => {
    ["placeholder", "title", "aria-label", "data-loading-text", "alt"].forEach((attribute) => {
      const value = element.getAttribute(attribute);
      if (!value) {
        return;
      }

      const translated = translate(value);
      if (translated) {
        element.setAttribute(attribute, translated);
      }
    });

    if (element instanceof HTMLInputElement && ["button", "submit", "reset"].includes(element.type)) {
      const translated = translate(element.value);
      if (translated) {
        element.value = translated;
      }
    }
  });

  const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, {
    acceptNode(node) {
      const parent = node.parentElement;
      if (!parent || skippedParents.has(parent.tagName) || !normalize(node.nodeValue || "")) {
        return NodeFilter.FILTER_REJECT;
      }

      return NodeFilter.FILTER_ACCEPT;
    }
  });

  const nodes = [];
  while (walker.nextNode()) {
    nodes.push(walker.currentNode);
  }

  nodes.forEach((node) => {
    const value = node.nodeValue || "";
    const translated = translate(value);
    if (translated) {
      node.nodeValue = value.replace(normalize(value), translated);
    }
  });
})();
