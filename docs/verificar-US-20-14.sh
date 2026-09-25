#!/bin/zsh
# Verificación de solo lectura de US-20-14 (datos sensibles en auditoría). No modifica archivos.
cd /Users/luisortiz/Desarrollo/DANMAOB/DANMAOB_TISAX/Net || exit 1

ok=0; fallas=0
revisa() {
  if [[ "$2" == "$3" ]]; then echo "OK    $1"; ok=$((ok+1))
  else echo "FALLA $1 (obtenido: $2, esperado: $3)"; fallas=$((fallas+1)); fi
}
cuenta() { local c; c=$(grep -c -- "$1" "$2" 2>/dev/null); echo "${c:-0}"; }

A=src/DanmaobTisax.Domain/Auditing/AuditRedactedAttribute.cs
revisa "Atributo AuditRedacted existe" "$([[ -f $A ]] && echo si || echo no)" "si"
revisa "Atributo solo para propiedades" "$(cuenta 'AttributeTargets.Property' $A)" "1"
revisa "User.PasswordHash marcado" "$(grep -A1 '\[AuditRedacted\]' src/DanmaobTisax.Domain/Identity/User.cs | grep -c 'PasswordHash')" "1"
revisa "PlatformAdministrator.PasswordHash marcado" "$(grep -A1 '\[AuditRedacted\]' src/DanmaobTisax.Domain/Identity/PlatformAdministrator.cs | grep -c 'PasswordHash')" "1"

S=src/DanmaobTisax.Infrastructure/Auditing/AuditValueSerializer.cs
revisa "Serializador enmascara en 3 lugares" "$(cuenta 'IsRedacted(property) == true ? RedactedValue' $S)" "3"
revisa "Valor de enmascarado exacto" "$(cuenta 'RedactedValue = "\[REDACTED\]"' $S)" "1"
revisa "Lista de columnas modificadas sin enmascarar" "$(sed -n '/public static string SerializeModifiedPropertyNames/,/^    }/p' $S | grep -c 'Redacted')" "0"

T=tests/DanmaobTisax.Infrastructure.IntegrationTests/Auditing/AuditRedactionTests.cs
revisa "Pruebas de enmascarado (3)" "$(cuenta '\[Fact\]' $T)" "3"

echo ""
echo "Resultado: $ok OK, $fallas FALLA"
echo "Pruebas esperadas al cerrar US-20-14: ArchitectureTests 4 + IntegrationTests 156 = 160."
