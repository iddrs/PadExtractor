WITH empenhos_rp AS (
	SELECT DISTINCT
		remessa,
		orgao,
		uniorcam,
		funcao,
		subfuncao,
		programa,
		projativ,
		rubrica,
		recurso_vinculado,
		contrapartida_recurso_vinculado,
		chave_empenho,
		ano_empenho,
		entidade_empenho,
		nr_empenho,
		credor,
		caracteristica_peculiar_despesa,
		complemento_recurso_vinculado,
		indicador_exercicio_fonte_recurso,
		fonte_recurso,
		codigo_acompanhamento_orcamentario,
		entidade
	FROM PAD.empenho
	WHERE
		remessa = %remessa%
		AND ano_empenho < %anoAtual%
),
vl_empenhado_pago AS (
	SELECT empenhos_rp.*,
	(
		SELECT SUM(valor_empenho)
		FROM PAD.empenho t
		WHERE remessa = %remessa%
		AND chave_empenho = empenhos_rp.chave_empenho
		AND data_empenho < '%dataInicial%'
	) AS empenhado,
	COALESCE((
		SELECT SUM(valor_pagamento)
		FROM PAD.pagament t
		WHERE remessa = %remessa%
		AND chave_empenho = empenhos_rp.chave_empenho
		AND data_pagamento < '%dataInicial%'
	), 0::money) AS pago
	FROM empenhos_rp
),
rp_saldo_inicial AS (
SELECT vl_empenhado_pago.*,
(
	SELECT SUM(empenhado - pago)
	FROM vl_empenhado_pago t
	WHERE t.chave_empenho = vl_empenhado_pago.chave_empenho
) AS rp_saldo_inicial
FROM vl_empenhado_pago
),
vl_liquidado AS (
	SELECT rp_saldo_inicial.*,
	COALESCE((
		SELECT SUM(valor_liquidacao)
		FROM PAD.liquidac t
		WHERE remessa = %remessa%
		AND t.chave_empenho = rp_saldo_inicial.chave_empenho
		AND t.data_liquidacao < '%dataInicial%'
	), 0::money) AS liquidado
	FROM rp_saldo_inicial
),
saldo_inicial_nao_processado AS (
	SELECT vl_liquidado.*,
	(
		SELECT SUM(t.empenhado - t.liquidado)
		FROM vl_liquidado t
		WHERE t.chave_empenho = vl_liquidado.chave_empenho
	) AS saldo_inicial_nao_processado
	FROM vl_liquidado
),
vl_empenhado_liquidado_pago_exercicios_anteriores AS (
	SELECT saldo_inicial_nao_processado.*,
	COALESCE((
		SELECT SUM(t.valor_empenho)
		FROM PAD.empenho t
		WHERE remessa = %remessa%
		AND data_empenho < '%dataInicialAnoAnterior%'
		AND t.chave_empenho = saldo_inicial_nao_processado.chave_empenho
	), 0::money) AS empenhado_exercicios_anteriores,
	COALESCE((
		SELECT SUM(t.valor_liquidacao)
		FROM PAD.liquidac t
		WHERE remessa = %remessa%
		AND data_liquidacao < '%dataInicialAnoAnterior%'
		AND t.chave_empenho = saldo_inicial_nao_processado.chave_empenho
	), 0::money) AS liquidado_exercicios_anteriores,
	COALESCE((
		SELECT SUM(t.valor_pagamento)
		FROM PAD.pagament t
		WHERE remessa = %remessa%
		AND data_pagamento < '%dataInicialAnoAnterior%'
		AND t.chave_empenho = saldo_inicial_nao_processado.chave_empenho
	), 0::money) AS pago_exercicios_anteriores
	FROM saldo_inicial_nao_processado
),
saldo_nao_processado_inscritos_exercicios_anteriores AS (
	SELECT vl_empenhado_liquidado_pago_exercicios_anteriores.*,
	(
		SELECT (t.empenhado_exercicios_anteriores - t.liquidado_exercicios_anteriores)
		FROM vl_empenhado_liquidado_pago_exercicios_anteriores t
		WHERE t.chave_empenho = vl_empenhado_liquidado_pago_exercicios_anteriores.chave_empenho
	) AS saldo_nao_processado_inscritos_exercicios_anteriores
	FROM vl_empenhado_liquidado_pago_exercicios_anteriores
),
vl_empenhado_liquidado_pago_ultimo_exercicio AS (
	SELECT saldo_nao_processado_inscritos_exercicios_anteriores.*,
	COALESCE((
		SELECT SUM(t.valor_empenho)
		FROM PAD.empenho t
		WHERE remessa = %remessa%
		AND data_empenho BETWEEN '%dataInicialAnoAnterior%' AND '%dataFinalAnoAnterior%'
		AND t.chave_empenho = saldo_nao_processado_inscritos_exercicios_anteriores.chave_empenho
	), 0::money) AS empenhado_ultimo_exercicio,
	COALESCE((
		SELECT SUM(t.valor_liquidacao)
		FROM PAD.liquidac t
		WHERE remessa = %remessa%
		AND data_liquidacao BETWEEN '%dataInicialAnoAnterior%' AND '%dataFinalAnoAnterior%'
		AND t.chave_empenho = saldo_nao_processado_inscritos_exercicios_anteriores.chave_empenho
	), 0::money) AS liquidado_ultimo_exercicio,
	COALESCE((
		SELECT SUM(t.valor_pagamento)
		FROM PAD.pagament t
		WHERE remessa = %remessa%
		AND data_pagamento BETWEEN '%dataInicialAnoAnterior%' AND '%dataFinalAnoAnterior%'
		AND t.chave_empenho = saldo_nao_processado_inscritos_exercicios_anteriores.chave_empenho
	), 0::money) AS pago_ultimo_exercicio
	FROM saldo_nao_processado_inscritos_exercicios_anteriores
),
nao_processado_inscritos_ultimo_exercicio AS (
	SELECT vl_empenhado_liquidado_pago_ultimo_exercicio.*,
	(
		SELECT (t.empenhado_ultimo_exercicio - t.liquidado_ultimo_exercicio)
		FROM vl_empenhado_liquidado_pago_ultimo_exercicio t
		WHERE t.chave_empenho = vl_empenhado_liquidado_pago_ultimo_exercicio.chave_empenho
	) AS nao_processado_inscritos_ultimo_exercicio
	FROM vl_empenhado_liquidado_pago_ultimo_exercicio
),
saldo_inicial_processado AS (
	SELECT nao_processado_inscritos_ultimo_exercicio.*,
	(
		SELECT (t.liquidado - t.pago)
		FROM nao_processado_inscritos_ultimo_exercicio t
		WHERE t.chave_empenho = nao_processado_inscritos_ultimo_exercicio.chave_empenho
	) AS saldo_inicial_processado
	FROM nao_processado_inscritos_ultimo_exercicio
),
saldo_processado_inscritos_exercicios_anteriores AS (
	SELECT saldo_inicial_processado.*,
	(
		SELECT (t.liquidado_exercicios_anteriores - t.pago_exercicios_anteriores)
		FROM saldo_inicial_processado t
		WHERE t.chave_empenho = saldo_inicial_processado.chave_empenho
	) AS saldo_processado_inscritos_exercicios_anteriores
	FROM saldo_inicial_processado
),
processado_inscritos_ultimo_exercicio AS (
	SELECT saldo_processado_inscritos_exercicios_anteriores.*,
	(
		SELECT (t.liquidado_ultimo_exercicio - t.pago_ultimo_exercicio)
		FROM saldo_processado_inscritos_exercicios_anteriores t
		WHERE t.chave_empenho = saldo_processado_inscritos_exercicios_anteriores.chave_empenho
	) AS processado_inscritos_ultimo_exercicio
	FROM saldo_processado_inscritos_exercicios_anteriores
),
rp_liquidado AS (
	SELECT processado_inscritos_ultimo_exercicio.*,
	COALESCE((
		SELECT
			SUM(t.valor_liquidacao)
		FROM PAD.liquidac t
		WHERE
			t.remessa = %remessa%
			AND t.chave_empenho = processado_inscritos_ultimo_exercicio.chave_empenho
			AND t.data_liquidacao BETWEEN '%dataInicial%' AND '%dataFinal%'
			AND t.valor_liquidacao::DECIMAL > 0.0
	), 0::money) AS rp_liquidado
	FROM processado_inscritos_ultimo_exercicio
),
processado_cancelado AS (
	SELECT rp_liquidado.*,
	(
		SELECT
			COALESCE(SUM(l.valor_liquidacao), 0::money)
		FROM PAD.liquidac l
		WHERE
			l.remessa = %remessa%
			AND l.data_liquidacao BETWEEN '%dataInicial%' AND '%dataFinal%'
			AND l.valor_liquidacao::DECIMAL < 0.0
			AND l.chave_empenho = rp_liquidado.chave_empenho
			AND (l.valor_liquidacao::DECIMAL*-1) IN (
				SELECT DISTINCT
					t.valor_liquidacao::decimal
				FROM PAD.liquidac t
				WHERE 
					t.chave_empenho = rp_liquidado.chave_empenho
					AND t.valor_liquidacao::DECIMAL > 0.0
					AND t.data_liquidacao < '%dataInicial%'
			)
	)*-1 AS processado_cancelado
	FROM rp_liquidado
),
rp_cancelado AS (
	SELECT processado_cancelado.*,
	COALESCE((
		SELECT 
			SUM(t.valor_empenho*-1)
		FROM PAD.empenho t
		WHERE remessa = %remessa%
		AND t.valor_empenho::DECIMAL < 0.0
		AND t.chave_empenho = processado_cancelado.chave_empenho
		AND t.data_empenho BETWEEN '%dataInicial%' AND '%dataFinal%'
	), 0::money) AS rp_cancelado
	FROM processado_cancelado
),
nao_processado_cancelado AS (
	SELECT 
		rp_cancelado.*,
		COALESCE((rp_cancelado - processado_cancelado), 0::money) AS nao_processado_cancelado
	FROM rp_cancelado
),
nao_processado_pago AS (
	SELECT nao_processado_cancelado.*,
	(
		SELECT
			COALESCE(SUM(p.valor_pagamento), 0::money)
		FROM PAD.pagament p
		WHERE
			p.remessa = %remessa%
			AND p.data_pagamento BETWEEN '%dataInicial%' AND '%dataFinal%'
			AND p.chave_empenho = nao_processado_cancelado.chave_empenho
			AND (p.valor_pagamento::DECIMAL) IN (
				SELECT DISTINCT
					t.valor_liquidacao::DECIMAL
				FROM PAD.liquidac t
				WHERE 
					t.chave_empenho = p.chave_empenho
					AND t.data_liquidacao BETWEEN '%dataInicial%' AND '%dataFinal%'
					AND t.remessa = %remessa%
			)
	) AS nao_processado_pago
	FROM nao_processado_cancelado
),
rp_pago AS (
	SELECT 
		nao_processado_pago.*,
		COALESCE((
			SELECT 
				SUM(t.valor_pagamento)
			FROM PAD.pagament t
			WHERE remessa = %remessa%
			AND t.data_pagamento BETWEEN '%dataInicial%' AND '%dataFinal%'
			AND t.chave_empenho = nao_processado_pago.chave_empenho
		), 0::money) AS rp_pago
	FROM nao_processado_pago
),
processado_pago AS (
	SELECT 
		rp_pago.*,
		(rp_pago - nao_processado_pago) AS processado_pago
	FROM rp_pago
),
saldo_final_nao_processado AS (
	SELECT processado_pago.*,
	(saldo_inicial_nao_processado - nao_processado_cancelado - nao_processado_pago) AS saldo_final_nao_processado
	FROM processado_pago
),
saldo_final_processado AS (
	SELECT saldo_final_nao_processado.*,
--	(saldo_inicial_processado - processado_cancelado - processado_pago + (rp_liquidado - nao_processado_pago)) AS saldo_final_processado
	(saldo_inicial_processado - processado_cancelado - processado_pago) AS saldo_final_processado
	FROM saldo_final_nao_processado
),
rp_saldo_final AS (
	SELECT saldo_final_processado.*,
	(saldo_final_processado +saldo_final_nao_processado) AS rp_saldo_final
	FROM saldo_final_processado
)
INSERT INTO pad.restos_pagar (
	remessa,
	orgao,
	uniorcam,
	funcao,
	subfuncao,
	programa,
	projativ,
	rubrica,
	recurso_vinculado,
	contrapartida_recurso_vinculado,
	chave_empenho,
	ano_empenho,
	entidade_empenho,
	nr_empenho,
	credor,
	caracteristica_peculiar_despesa,
	complemento_recurso_vinculado,
	indicador_exercicio_fonte_recurso,
	fonte_recurso,
	codigo_acompanhamento_orcamentario,
	entidade,
	saldo_nao_processado_inscritos_exercicios_anteriores,
	nao_processado_inscritos_ultimo_exercicio,
	saldo_inicial_nao_processado,
	saldo_processado_inscritos_exercicios_anteriores,
	processado_inscritos_ultimo_exercicio,
	saldo_inicial_processado,
	rp_liquidado,
	rp_pago,
	nao_processado_cancelado,
	processado_cancelado,
	nao_processado_pago,
	processado_pago,
	saldo_final_nao_processado,
	saldo_final_processado,
	rp_saldo_inicial,
	rp_saldo_final,
	rp_cancelado,
	empenhado,
	liquidado,
	pago
)
SELECT 
	remessa,
	orgao,
	uniorcam,
	funcao,
	subfuncao,
	programa,
	projativ,
	rubrica,
	recurso_vinculado,
	contrapartida_recurso_vinculado,
	chave_empenho,
	ano_empenho,
	entidade_empenho,
	nr_empenho,
	credor,
	caracteristica_peculiar_despesa,
	complemento_recurso_vinculado,
	indicador_exercicio_fonte_recurso,
	fonte_recurso,
	codigo_acompanhamento_orcamentario,
	entidade,
	saldo_nao_processado_inscritos_exercicios_anteriores,
	nao_processado_inscritos_ultimo_exercicio,
	saldo_inicial_nao_processado,
	saldo_processado_inscritos_exercicios_anteriores,
	processado_inscritos_ultimo_exercicio,
	saldo_inicial_processado,
	rp_liquidado,
	rp_pago,
	nao_processado_cancelado,
	processado_cancelado,
	nao_processado_pago,
	processado_pago,
	saldo_final_nao_processado,
	saldo_final_processado,
	rp_saldo_inicial,
	rp_saldo_final,
	rp_cancelado,
	empenhado,
	liquidado,
	pago
FROM rp_saldo_final;